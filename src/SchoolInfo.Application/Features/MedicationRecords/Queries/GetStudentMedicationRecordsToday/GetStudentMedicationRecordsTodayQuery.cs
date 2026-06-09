using System;
using System.Collections.Generic;
using MediatR;

namespace SchoolInfo.Application.Features.MedicationRecords.Queries.GetStudentMedicationRecordsToday;

public record MedicationRecordDto(
    Guid Id,
    Guid DailyRecordId,
    Guid StudentId,
    string MedicineName,
    string Dosage,
    string AdministrationTime,
    bool Taken,
    string? Note);

public record GetStudentMedicationRecordsTodayQuery(Guid StudentId) : IRequest<List<MedicationRecordDto>>;
