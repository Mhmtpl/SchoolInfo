using System;
using System.Collections.Generic;
using MediatR;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomDailyRecords;

public record ClassroomDailyRecordDto(
    Guid StudentId, 
    string FirstName, 
    string LastName, 
    bool HasRecordToday,
    SleepStatus? SleepStatus,
    int? WaterIntake,
    string TeacherNotes,
    bool IsAbsent
);

public record GetClassroomDailyRecordsQuery(Guid ClassroomId) : IRequest<List<ClassroomDailyRecordDto>>;
