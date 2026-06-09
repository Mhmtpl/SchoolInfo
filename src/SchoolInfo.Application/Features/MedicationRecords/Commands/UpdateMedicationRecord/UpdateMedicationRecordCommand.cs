using System;
using MediatR;

namespace SchoolInfo.Application.Features.MedicationRecords.Commands.UpdateMedicationRecord;

public record UpdateMedicationRecordCommand(
    Guid Id,
    string MedicineName,
    string Dosage,
    string AdministrationTime,
    bool Taken,
    string? Note) : IRequest;
