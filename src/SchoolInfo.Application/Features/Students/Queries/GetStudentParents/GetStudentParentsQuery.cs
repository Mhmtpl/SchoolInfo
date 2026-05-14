using System;
using System.Collections.Generic;
using MediatR;

namespace SchoolInfo.Application.Features.Students.Queries.GetStudentParents;

public record ParentDto(Guid Id, string FirstName, string LastName, string Email);

public record GetStudentParentsQuery(Guid StudentId) : IRequest<List<ParentDto>>;
