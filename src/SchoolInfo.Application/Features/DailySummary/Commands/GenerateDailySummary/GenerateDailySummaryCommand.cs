using System;
using MediatR;

namespace SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

/// <summary>
/// GÃ¼nlÃ¼k Ã¶zet raporu oluÅŸturma isteÄŸi.
/// </summary>
public record GenerateDailySummaryCommand(Guid StudentId, DateTime Date) : IRequest<Guid>;
