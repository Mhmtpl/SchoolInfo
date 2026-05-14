using System;
using MediatR;

namespace SchoolInfo.Application.Features.Students.Commands.DeleteStudent;

public record DeleteStudentCommand(Guid Id) : IRequest;
