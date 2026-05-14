using System;
using MediatR;

namespace SchoolInfo.Application.Features.Classrooms.Commands.DeleteClassroom;

public record DeleteClassroomCommand(Guid Id) : IRequest;
