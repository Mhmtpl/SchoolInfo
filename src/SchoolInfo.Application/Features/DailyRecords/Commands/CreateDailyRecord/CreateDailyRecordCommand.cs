using System;
using MediatR;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.CreateDailyRecord;

/// <summary>
/// GÃ¼nlÃ¼k kayÄ±t oluÅŸturma isteÄŸi.
/// </summary>
public record CreateDailyRecordCommand(Guid StudentId, DateTime Date) : IRequest<Guid>;
