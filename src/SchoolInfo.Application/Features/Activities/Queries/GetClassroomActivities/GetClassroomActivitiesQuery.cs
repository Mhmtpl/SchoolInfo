using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Application.Features.Activities.Queries.GetActivity;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Activities.Queries.GetClassroomActivities;

/// <summary>
/// Belirtilen sınıfın tüm aktivitelerini tarihe göre azalan sırada listeler.
/// </summary>
public record GetClassroomActivitiesQuery(Guid ClassroomId) : IRequest<List<ActivityDto>>;

public class GetClassroomActivitiesQueryHandler : IRequestHandler<GetClassroomActivitiesQuery, List<ActivityDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetClassroomActivitiesQueryHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<ActivityDto>> Handle(GetClassroomActivitiesQuery request, CancellationToken cancellationToken)
    {
        // Önce sınıfın kullanıcının okuluyla eşleştiğini doğrula
        var classroomExists = await ((DbContext)_dbContext).Set<Classroom>()
            .AnyAsync(c => c.Id == request.ClassroomId && c.SchoolId == _currentUserService.SchoolId, cancellationToken);

        if (!classroomExists)
            throw new System.Collections.Generic.KeyNotFoundException("Sınıf bulunamadı veya bu sınıfa erişim yetkiniz yok.");

        var activities = await ((DbContext)_dbContext).Set<Activity>()
            .AsNoTracking()
            .Where(a => a.ClassroomId == request.ClassroomId && a.SchoolId == _currentUserService.SchoolId)
            .OrderByDescending(a => a.ActivityDate)
            .Select(a => new ActivityDto(
                a.Id,
                a.Title,
                a.Description,
                a.ActivityDate,
                a.StartTime,
                a.EndTime,
                a.Type,
                a.ClassroomId,
                a.CompletedAt
            ))
            .ToListAsync(cancellationToken);

        return activities;
    }
}
