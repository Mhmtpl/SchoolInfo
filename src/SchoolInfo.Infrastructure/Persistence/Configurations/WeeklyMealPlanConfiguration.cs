using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// WeeklyMealPlan entity'si için veritabanı yapılandırması.
/// </summary>
public class WeeklyMealPlanConfiguration : IEntityTypeConfiguration<WeeklyMealPlan>
{
    public void Configure(EntityTypeBuilder<WeeklyMealPlan> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.MealName).IsRequired().HasMaxLength(100);
        builder.Property(w => w.FoodContent).HasMaxLength(500);

        // Hızlı sorgulamalar için indeks
        builder.HasIndex(w => new { w.ClassroomId, w.DayOfWeek });
    }
}
