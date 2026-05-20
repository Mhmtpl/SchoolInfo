using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Sınıf bazlı varsayılan haftalık yemek menüsü şablonu.
/// </summary>
public class WeeklyMealPlan : BaseEntity
{
    public Guid ClassroomId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public string MealName { get; private set; } // "Kahvaltı", "Öğle Yemeği", "İkindi Kahvaltısı"
    public int PlannedCalories { get; private set; }
    public string FoodContent { get; private set; }
    public double ProteinGrams { get; private set; }
    public double CarbsGrams { get; private set; }

    protected WeeklyMealPlan() { } // EF Core için

    public WeeklyMealPlan(Guid classroomId, DayOfWeek dayOfWeek, string mealName, int plannedCalories, string foodContent, double proteinGrams, double carbsGrams, Guid schoolId)
    {
        ClassroomId = classroomId;
        DayOfWeek = dayOfWeek;
        MealName = mealName;
        PlannedCalories = plannedCalories;
        FoodContent = foodContent;
        ProteinGrams = proteinGrams;
        CarbsGrams = carbsGrams;
        SchoolId = schoolId;
    }

    /// <summary>
    /// Şablon menünün değerlerini günceller.
    /// </summary>
    public void UpdatePlan(int plannedCalories, string foodContent, double proteinGrams, double carbsGrams)
    {
        PlannedCalories = plannedCalories;
        FoodContent = foodContent;
        ProteinGrams = proteinGrams;
        CarbsGrams = carbsGrams;
        UpdateTimestamp();
    }
}
