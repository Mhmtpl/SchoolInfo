using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Events;

/// <summary>
/// GÃ¼n sonu Ã¶zeti istendiÄŸinde fÄ±rlatÄ±lan olay.
/// </summary>
/// <param name="SummaryId">Ä°stenen Ã¶zetin Id'si.</param>
public record DailySummaryRequestedEvent(Guid SummaryId) : IDomainEvent;
