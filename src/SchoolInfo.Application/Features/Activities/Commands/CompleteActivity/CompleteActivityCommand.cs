using System;
using MediatR;

namespace SchoolInfo.Application.Features.Activities.Commands.CompleteActivity;

/// <summary>
/// Aktivite tamamlama isteÄŸi.
/// </summary>
public record CompleteActivityCommand(Guid ActivityId) : IRequest;
