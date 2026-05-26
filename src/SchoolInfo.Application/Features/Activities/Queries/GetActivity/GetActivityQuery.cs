using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Activities.Queries.GetActivity;

/// <summary>
/// Belirtilen ID'ye sahip aktivitenin detaylarını getirir.
/// </summary>
public record GetActivityQuery(Guid Id) : IRequest<ActivityDto>;

public record ActivityDto(
    Guid Id,
    string Title,
    string Description,
    DateTime ActivityDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    SchoolInfo.Domain.Enums.ActivityType Type,
    Guid ClassroomId,
    DateTime? CompletedAt
);

public class GetActivityQueryHandler : IRequestHandler<GetActivityQuery, ActivityDto>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetActivityQueryHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ActivityDto> Handle(GetActivityQuery request, CancellationToken cancellationToken)
    {
        var activity = await ((DbContext)_dbContext).Set<Activity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (activity == null)
            throw new System.Collections.Generic.KeyNotFoundException("Aktivite bulunamadı.");

        return new ActivityDto(
            activity.Id,
            activity.Title,
            activity.Description,
            activity.ActivityDate,
            activity.StartTime,
            activity.EndTime,
            activity.Type,
            activity.ClassroomId,
            activity.CompletedAt
        );
    }
}
