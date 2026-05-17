using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomDailyRecords;

public class GetClassroomDailyRecordsHandler : IRequestHandler<GetClassroomDailyRecordsQuery, List<ClassroomDailyRecordDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetClassroomDailyRecordsHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<ClassroomDailyRecordDto>> Handle(GetClassroomDailyRecordsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var students = await _dbContext.Students
            .Where(s => s.ClassroomId == request.ClassroomId && s.SchoolId == _currentUserService.SchoolId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var studentIds = students.Select(s => s.Id).ToList();

        var dailyRecords = await _dbContext.DailyRecords
            .Where(r => studentIds.Contains(r.StudentId) && r.Date == today)
            .AsNoTracking()
            .ToDictionaryAsync(r => r.StudentId, cancellationToken);

        var result = students.Select(student =>
        {
            var hasRecord = dailyRecords.TryGetValue(student.Id, out var record);
            return new ClassroomDailyRecordDto(
                student.Id,
                student.FirstName,
                student.LastName,
                hasRecord,
                hasRecord ? record.SleepInfo.Status : null,
                hasRecord ? record.WaterConsumption.AmountInMilliliters : null,
                hasRecord ? record.TeacherNote : null
            );
        }).ToList();

        return result;
    }
}
