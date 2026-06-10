using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Application.Features.MedicationRecords.Queries.GetStudentMedicationRecordsToday;

namespace SchoolInfo.Application.Features.MedicationRecords.Queries.GetClassroomMedicationRecordsToday;

public class GetClassroomMedicationRecordsTodayQueryHandler : IRequestHandler<GetClassroomMedicationRecordsTodayQuery, List<MedicationRecordDto>>
{
    private readonly IAppDbContext _dbContext;

    public GetClassroomMedicationRecordsTodayQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MedicationRecordDto>> Handle(GetClassroomMedicationRecordsTodayQuery request, CancellationToken cancellationToken)
    {
        var today = request.Date.HasValue ? DateTime.SpecifyKind(request.Date.Value.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
        
        var studentIds = await _dbContext.Students
            .Where(s => s.ClassroomId == request.ClassroomId && !s.IsDeleted)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var records = await _dbContext.MedicationRecords
            .Where(m => studentIds.Contains(m.StudentId) && !m.IsDeleted)
            .Where(m => _dbContext.DailyRecords.Any(d => d.Id == m.DailyRecordId && d.Date == today && !d.IsDeleted))
            .ToListAsync(cancellationToken);

        return records.Select(m => new MedicationRecordDto(
            m.Id,
            m.DailyRecordId,
            m.StudentId,
            m.MedicineName,
            m.Dosage,
            m.AdministrationTime,
            m.Taken,
            m.Note
        )).ToList();
    }
}
