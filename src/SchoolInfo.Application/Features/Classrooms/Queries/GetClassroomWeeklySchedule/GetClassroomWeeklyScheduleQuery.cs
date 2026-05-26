using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Application.Features.Classrooms.Commands.UpdateClassroomWeeklySchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomWeeklySchedule;

public record GetClassroomWeeklyScheduleQuery(Guid ClassroomId) : IRequest<List<WeeklyScheduleDto>>;

public class GetClassroomWeeklyScheduleQueryHandler : IRequestHandler<GetClassroomWeeklyScheduleQuery, List<WeeklyScheduleDto>>
{
    private readonly IAppDbContext _dbContext;

    public GetClassroomWeeklyScheduleQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WeeklyScheduleDto>> Handle(GetClassroomWeeklyScheduleQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _dbContext.ClassroomWeeklySchedules
            .Where(x => x.ClassroomId == request.ClassroomId)
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime)
            .Select(x => new WeeklyScheduleDto(
                x.DayOfWeek,
                x.Title,
                x.Description,
                x.StartTime,
                x.EndTime,
                (int)x.Type
            ))
            .ToListAsync(cancellationToken);

        return schedules;
    }
}
