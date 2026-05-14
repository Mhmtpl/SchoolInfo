using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.Activities.Commands.CreateActivity;

/// <summary>
/// Yeni aktivite oluşturma işlemini yürüten sınıf.
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

        var activity = new Activity(request.Title, request.Description, request.ActivityDate, request.ClassroomId);
        
        await _activityRepository.AddAsync(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return activity.Id;
    }
}
