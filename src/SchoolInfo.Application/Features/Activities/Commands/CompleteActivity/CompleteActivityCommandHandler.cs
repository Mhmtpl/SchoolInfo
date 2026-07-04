using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Exceptions;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.Activities.Commands.CompleteActivity;

/// <summary>
/// Aktivite tamamlama işlemini yürüten sınıf.
/// </summary>
public class CompleteActivityCommandHandler : IRequestHandler<CompleteActivityCommand>
{
    private readonly IActivityRepository _activityRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CompleteActivityCommandHandler(
        IActivityRepository activityRepository, 
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _activityRepository = activityRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CompleteActivityCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Aktiviteyi tamamlamak için yetkiniz bulunmamaktadır.");
        }

        var activity = await _activityRepository.GetByIdAsync(request.ActivityId);
        if (activity == null)
        {
            throw new DomainException("Aktivite bulunamadı.");
        }

        if (activity.SchoolId != _currentUserService.SchoolId)
        {
            throw new UnauthorizedAccessException("Bu aktiviteyi tamamlamak için yetkiniz bulunmamaktadır.");
        }

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await ((DbContext)_dbContext).Set<Classroom>()
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == activity.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu sınıfın aktivitesini tamamlamak için yetkiniz bulunmamaktadır.");
            }
        }

        activity.Complete();
        
        await _activityRepository.UpdateAsync(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}


