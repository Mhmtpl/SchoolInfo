using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.Activities.Commands.CreateActivity;

/// <summary>
/// Yeni aktivite oluÅŸturma iÅŸlemini yÃ¼rÃ¼ten sÄ±nÄ±f.
/// </summary>
public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, Guid>
{
    private readonly IActivityRepository _activityRepository;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateActivityCommandHandler(
        IActivityRepository activityRepository, 
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _activityRepository = activityRepository;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Aktivite oluşturmak için yetkiniz bulunmamaktadır.");
        }

        // Sınıfın okulla eşleştiğini doğrula ve öğretmenin bu sınıfa atanıp atanmadığını kontrol et
        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await ((DbContext)_dbContext).Set<Classroom>()
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == request.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu sınıfta etkinlik oluşturmak için yetkiniz bulunmamaktadır.");
            }
        }
        else if (_currentUserService.Role == "Admin")
        {
            var classroomExists = await ((DbContext)_dbContext).Set<Classroom>()
                .AnyAsync(c => c.Id == request.ClassroomId && c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted, cancellationToken);

            if (!classroomExists)
            {
                throw new UnauthorizedAccessException("Sınıf bulunamadı veya bu sınıfa erişim yetkiniz yok.");
            }
        }

        var activity = new Activity(request.Title, request.Description, request.ActivityDate, request.StartTime, request.EndTime, request.Type, request.ClassroomId);
        activity.SchoolId = _currentUserService.SchoolId;
        
        await _activityRepository.AddAsync(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return activity.Id;
    }

}
