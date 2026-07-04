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
    private readonly ICurrentUserService _currentUserService;

    public GetClassroomWeeklyScheduleQueryHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<WeeklyScheduleDto>> Handle(GetClassroomWeeklyScheduleQuery request, CancellationToken cancellationToken)
    {
        var classroom = await _dbContext.Classrooms
            .Include(c => c.Teachers)
            .Include(c => c.Students)
                .ThenInclude(s => s.Parents)
            .FirstOrDefaultAsync(c => c.Id == request.ClassroomId && c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted, cancellationToken);

        if (classroom == null)
        {
            throw new KeyNotFoundException("Sınıf bulunamadı veya bu sınıfa erişim yetkiniz yok.");
        }

        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = classroom.Teachers.Any(t => t.Id == _currentUserService.UserId);
            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Atanmadığınız bir sınıfın haftalık ders programını görüntüleyemezsiniz.");
            }
        }
        else if (_currentUserService.Role == "Parent")
        {
            var hasChild = classroom.Students.Any(s => s.Parents.Any(p => p.Id == _currentUserService.UserId));
            if (!hasChild)
            {
                throw new UnauthorizedAccessException("Çocuğunuzun bulunmadığı bir sınıfın haftalık ders programını görüntüleyemezsiniz.");
            }
        }
        else if (_currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Haftalık ders programını görüntülemek için yetkiniz bulunmamaktadır.");
        }

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
