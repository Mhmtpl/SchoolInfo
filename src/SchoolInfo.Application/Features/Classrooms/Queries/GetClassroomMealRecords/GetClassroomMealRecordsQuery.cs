using System;
using System.Collections.Generic;
using MediatR;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomMealRecords;

public record StudentMealDto(
    Guid StudentId,
    string FirstName,
    string LastName,
    Guid? MealRecordId,
    MealStatusType? Status,
    string Notes
);

public record GetClassroomMealRecordsQuery(Guid ClassroomId, DateTime? Date = null) : IRequest<List<StudentMealDto>>;
