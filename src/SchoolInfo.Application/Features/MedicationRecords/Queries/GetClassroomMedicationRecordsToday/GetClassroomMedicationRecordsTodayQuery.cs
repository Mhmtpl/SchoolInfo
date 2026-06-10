using System;
using System.Collections.Generic;
using MediatR;
using SchoolInfo.Application.Features.MedicationRecords.Queries.GetStudentMedicationRecordsToday;

namespace SchoolInfo.Application.Features.MedicationRecords.Queries.GetClassroomMedicationRecordsToday;

public record GetClassroomMedicationRecordsTodayQuery(Guid ClassroomId, DateTime? Date = null) : IRequest<List<MedicationRecordDto>>;
