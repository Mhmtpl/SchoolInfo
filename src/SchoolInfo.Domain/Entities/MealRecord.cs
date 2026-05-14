using System;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.ValueObjects;
using SchoolInfo.Domain.Events;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Öğrencinin öğün kaydı.
/// </summary>
public class MealRecord : BaseEntity
{
    public Guid DailyRecordId { get; private set; }
    public string MealName { get; private set; }
    public MealStatus Status { get; private set; }

    protected MealRecord() { } // EF Core için

    public MealRecord(Guid dailyRecordId, string mealName, MealStatus status)
    {
        DailyRecordId = dailyRecordId;
        MealName = mealName;
        Status = status;
    }

    /// <summary>
    /// Öğün durumunu günceller.
    /// </summary>
    public void UpdateStatus(MealStatus newStatus)
    {
        Status = newStatus;
        UpdateTimestamp();
        
        AddDomainEvent(new MealRecordUpdatedEvent(Id));
    }
}
