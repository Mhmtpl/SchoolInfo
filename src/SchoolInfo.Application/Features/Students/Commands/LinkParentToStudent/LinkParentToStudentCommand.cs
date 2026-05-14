using System;
using MediatR;

namespace SchoolInfo.Application.Features.Students.Commands.LinkParentToStudent;

public record LinkParentToStudentCommand(Guid StudentId, Guid ParentId) : IRequest;
