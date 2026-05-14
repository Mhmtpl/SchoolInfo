using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomMealRecords;

public class GetClassroomMealRecordsHandler : IRequestHandler<GetClassroomMealRecordsQuery, List<StudentMealDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetClassroomMealRecordsHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<StudentMealDto>> Handle(GetClassroomMealRecordsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var students = await _dbContext.Students
            .Where(s => s.ClassroomId == request.ClassroomId && s.SchoolId == _currentUserService.SchoolId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var studentIds = students.Select(s => s.Id).ToList();

        var dailyRecords = await _dbContext.DailyRecords
            .Where(r => studentIds.Contains(r.StudentId) && r.Date.Date == today)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dailyRecordIds = dailyRecords.Select(d => d.Id).ToList();

        var mealRecords = await _dbContext.MealRecords
            .Where(m => dailyRecordIds.Contains(m.DailyRecordId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = students.Select(student =>
        {
            var dailyRecord = dailyRecords.FirstOrDefault(d => d.StudentId == student.Id);
            if (dailyRecord != null)
            {
                var meal = mealRecords.FirstOrDefault(m => m.DailyRecordId == dailyRecord.Id);
                return new StudentMealDto(student.Id, student.FirstName, student.LastName, meal?.Id, meal?.Status.Type, meal?.Status.Description);
            }
            
            return new StudentMealDto(student.Id, student.FirstName, student.LastName, null, null, null);
        }).ToList();

        return result;
    }
}
