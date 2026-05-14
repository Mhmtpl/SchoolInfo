using System;
using MediatR;

namespace SchoolInfo.Application.Features.Activities.Commands.CompleteActivity;

/// <summary>
/// Aktivite tamamlama isteği.
/// </summary>
public record CompleteActivityCommand(Guid ActivityId) : IRequest;
