using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.MedicationRecords.Queries.GetStudentMedicationRecordsToday;

public class GetStudentMedicationRecordsTodayQueryHandler : IRequestHandler<GetStudentMedicationRecordsTodayQuery, List<MedicationRecordDto>>
{
    private readonly IMedicationRecordRepository _medicationRecordRepository;

    public GetStudentMedicationRecordsTodayQueryHandler(IMedicationRecordRepository medicationRecordRepository)
    {
        _medicationRecordRepository = medicationRecordRepository;
    }

    public async Task<List<MedicationRecordDto>> Handle(GetStudentMedicationRecordsTodayQuery request, CancellationToken cancellationToken)
    {
        var today = request.Date.HasValue ? DateTime.SpecifyKind(request.Date.Value.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
        var records = await _medicationRecordRepository.GetByStudentAndDateAsync(request.StudentId, today);

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
