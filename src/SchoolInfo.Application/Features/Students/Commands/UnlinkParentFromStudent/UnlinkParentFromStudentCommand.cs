using System;
using MediatR;

namespace SchoolInfo.Application.Features.Students.Commands.UnlinkParentFromStudent;

public record UnlinkParentFromStudentCommand(Guid StudentId, Guid ParentId) : IRequest;
