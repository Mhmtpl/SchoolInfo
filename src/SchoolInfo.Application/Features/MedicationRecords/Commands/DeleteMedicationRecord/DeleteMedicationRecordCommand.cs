using System;
using MediatR;

namespace SchoolInfo.Application.Features.MedicationRecords.Commands.DeleteMedicationRecord;

public record DeleteMedicationRecordCommand(Guid Id) : IRequest;
