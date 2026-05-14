using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Exceptions;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.Activities.Commands.CompleteActivity;

/// <summary>
/// Aktivite tamamlama iÅŸlemini yÃ¼rÃ¼ten sÄ±nÄ±f.
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
            throw new UnauthorizedAccessException("Aktiviteyi tamamlamak iÃ§in yetkiniz bulunmamaktadÄ±r.");
        }

        var activity = await _activityRepository.GetByIdAsync(request.ActivityId);
        if (activity == null)
        {
            throw new DomainException("Aktivite bulunamadÄ±.");
        }

        // Domain iÃ§erisinde tamamlandÄ± iÅŸareti veya mantÄ±ÄŸÄ± varsa o tetiklenir
        // activity.MarkAsCompleted(); // Ã–rn
        
        await _activityRepository.UpdateAsync(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
