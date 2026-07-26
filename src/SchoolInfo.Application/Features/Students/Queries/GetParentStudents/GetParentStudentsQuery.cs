using System;
using System.Collections.Generic;
using MediatR;

namespace SchoolInfo.Application.Features.Students.Queries.GetParentStudents;

public record StudentDto(
    Guid Id, 
    string FirstName, 
    string LastName, 
    DateTime DateOfBirth, 
    Guid ClassroomId,
    string ClassroomName,
    string SchoolName
);

public record GetParentStudentsQuery(Guid ParentId) : IRequest<List<StudentDto>>;
