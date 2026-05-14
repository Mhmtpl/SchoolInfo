using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Events;

/// <summary>
/// Günlük kayıt oluşturulduğunda fırlatılan olay.
/// </summary>
/// <param name="DailyRecordId">Oluşturulan günlüğün Id'si.</param>
/// <param name="StudentId">Öğrencinin Id'si.</param>
public record DailyRecordCreatedEvent(Guid DailyRecordId, Guid StudentId) : IDomainEvent;
