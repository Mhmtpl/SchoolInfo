using System;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.ValueObjects;
using SchoolInfo.Domain.Events;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Ã–ÄŸrencinin Ã¶ÄŸÃ¼n kaydÄ±.
/// </summary>
public class MealRecord : BaseEntity
{
    public Guid DailyRecordId { get; private set; }
    public string MealName { get; private set; }
    public MealStatus Status { get; private set; }

    protected MealRecord() { } // EF Core iÃ§in

    public MealRecord(Guid dailyRecordId, string mealName, MealStatus status)
    {
        DailyRecordId = dailyRecordId;
        MealName = mealName;
        Status = status;
    }

    /// <summary>
    /// Ã–ÄŸÃ¼n durumunu gÃ¼nceller.
    /// </summary>
    public void UpdateStatus(MealStatus newStatus)
    {
        Status = newStatus;
        UpdateTimestamp();
        
        AddDomainEvent(new MealRecordUpdatedEvent(Id));
    }
}
