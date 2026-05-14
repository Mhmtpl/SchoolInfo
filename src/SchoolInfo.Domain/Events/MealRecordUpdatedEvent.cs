using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Events;

/// <summary>
/// Öğün kaydı güncellendiğinde fırlatılan olay.
/// </summary>
/// <param name="MealRecordId">Güncellenen öğün kaydının Id'si.</param>
public record MealRecordUpdatedEvent(Guid MealRecordId) : IDomainEvent;
