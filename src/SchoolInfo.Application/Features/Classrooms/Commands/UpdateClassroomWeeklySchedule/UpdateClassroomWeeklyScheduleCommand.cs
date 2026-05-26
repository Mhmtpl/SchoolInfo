using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Features.Classrooms.Commands.UpdateClassroomWeeklySchedule;

public record WeeklyScheduleDto(int DayOfWeek, string Title, string Description, TimeSpan StartTime, TimeSpan EndTime, int Type);

public record UpdateClassroomWeeklyScheduleCommand(Guid ClassroomId, List<WeeklyScheduleDto> Schedules) : IRequest<bool>;

public class UpdateClassroomWeeklyScheduleCommandHandler : IRequestHandler<UpdateClassroomWeeklyScheduleCommand, bool>
{
    private readonly IAppDbContext _dbContext;

    public UpdateClassroomWeeklyScheduleCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateClassroomWeeklyScheduleCommand request, CancellationToken cancellationToken)
    {
        var classroom = await _dbContext.Classrooms.FindAsync(new object[] { request.ClassroomId }, cancellationToken);
        if (classroom == null) return false;

        // Silinecekleri sil (eski şablon)
        var existingSchedules = await _dbContext.ClassroomWeeklySchedules
            .Where(x => x.ClassroomId == request.ClassroomId)
            .ToListAsync(cancellationToken);

        _dbContext.ClassroomWeeklySchedules.RemoveRange(existingSchedules);

        // Yenileri ekle
        if (request.Schedules != null && request.Schedules.Any())
        {
            var newSchedules = request.Schedules.Select(s => new ClassroomWeeklySchedule(
                request.ClassroomId,
                classroom.SchoolId,
                s.DayOfWeek,
                s.Title ?? "",
                s.Description ?? "",
                s.StartTime,
                s.EndTime,
                (ActivityType)s.Type
            )).ToList();

            await _dbContext.ClassroomWeeklySchedules.AddRangeAsync(newSchedules, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
