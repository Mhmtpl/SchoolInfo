using System;
using MediatR;

using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.Activities.Commands.CreateActivity;

/// <summary>
/// Yeni aktivite oluÅŸturma isteÄŸi.
/// </summary>
public record CreateActivityCommand(string Title, string Description, DateTime ActivityDate, TimeSpan StartTime, TimeSpan EndTime, ActivityType Type, Guid ClassroomId) : IRequest<Guid>;
