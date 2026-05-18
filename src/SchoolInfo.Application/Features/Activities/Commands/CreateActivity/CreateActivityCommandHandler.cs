using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
            throw new UnauthorizedAccessException("Aktivite oluÅŸturmak iÃ§in yetkiniz bulunmamaktadÄ±r.");
        }

        var activity = new Activity(request.Title, request.Description, request.ActivityDate, request.ClassroomId);
        activity.SchoolId = _currentUserService.SchoolId;
        
        await _activityRepository.AddAsync(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return activity.Id;
    }
}
