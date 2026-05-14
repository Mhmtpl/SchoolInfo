using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Events;

/// <summary>
/// Ã–ÄŸÃ¼n kaydÄ± gÃ¼ncellendiÄŸinde fÄ±rlatÄ±lan olay.
/// </summary>
/// <param name="MealRecordId">GÃ¼ncellenen Ã¶ÄŸÃ¼n kaydÄ±nÄ±n Id'si.</param>
public record MealRecordUpdatedEvent(Guid MealRecordId) : IDomainEvent;
