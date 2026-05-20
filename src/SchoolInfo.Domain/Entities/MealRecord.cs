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
    public int? PlannedCalories { get; private set; }
    public string? FoodContent { get; private set; }
    public double? ProteinGrams { get; private set; }
    public double? CarbsGrams { get; private set; }

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

    /// <summary>
    /// Öğünün besin ve kalori değerlerini tanımlar.
    /// </summary>
    public void SetNutrition(int calories, string foodContent, double protein, double carbs)
    {
        PlannedCalories = calories;
        FoodContent = foodContent;
        ProteinGrams = protein;
        CarbsGrams = carbs;
        UpdateTimestamp();
    }
}
