using System;
using MediatR;

namespace SchoolInfo.Application.Features.MedicationRecords.Commands.CreateMedicationRecord;

public record CreateMedicationRecordCommand(
    Guid StudentId,
    string MedicineName,
    string Dosage,
    string AdministrationTime,
    bool Taken,
    string? Note,
    DateTime? Date = null) : IRequest<Guid>;
