using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Events;

/// <summary>
/// GÃ¼nlÃ¼k kayÄ±t oluÅŸturulduÄŸunda fÄ±rlatÄ±lan olay.
/// </summary>
/// <param name="DailyRecordId">OluÅŸturulan gÃ¼nlÃ¼ÄŸÃ¼n Id'si.</param>
/// <param name="StudentId">Ã–ÄŸrencinin Id'si.</param>
public record DailyRecordCreatedEvent(Guid DailyRecordId, Guid StudentId) : IDomainEvent;
