using System;
using MediatR;

namespace SchoolInfo.Application.Features.Activities.Commands.CreateActivity;

/// <summary>
/// Yeni aktivite oluÅŸturma isteÄŸi.
/// </summary>
public record CreateActivityCommand(string Title, string Description, DateTime ActivityDate, Guid ClassroomId) : IRequest<Guid>;
