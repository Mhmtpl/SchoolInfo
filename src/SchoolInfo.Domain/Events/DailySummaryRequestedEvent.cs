using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Events;

/// <summary>
/// Gün sonu özeti istendiğinde fırlatılan olay.
/// </summary>
/// <param name="SummaryId">İstenen özetin Id'si.</param>
public record DailySummaryRequestedEvent(Guid SummaryId) : IDomainEvent;
