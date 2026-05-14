using System;
using MediatR;

namespace SchoolInfo.Application.Features.Activities.Commands.DeleteActivity;

public record DeleteActivityCommand(Guid Id) : IRequest;
