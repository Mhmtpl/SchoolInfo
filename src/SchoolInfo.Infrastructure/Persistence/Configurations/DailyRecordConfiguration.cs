using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// DailyRecord entity'si iÃ§in veritabanÄ± yapÄ±landÄ±rmasÄ±. Value Object'ler owned entity olarak map edilir.
/// </summary>
public class DailyRecordConfiguration : IEntityTypeConfiguration<DailyRecord>
{
    public void Configure(EntityTypeBuilder<DailyRecord> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Date).IsRequired();

        // Owned Entity: SleepData
        builder.OwnsOne(d => d.SleepInfo, sleep =>
        {
            sleep.Property(s => s.Status).HasColumnName("SleepStatus");
            sleep.Property(s => s.StartTime).HasColumnName("SleepStartTime");
            sleep.Property(s => s.EndTime).HasColumnName("SleepEndTime");
        });

        // Owned Entity: WaterIntake
        builder.OwnsOne(d => d.WaterConsumption, water =>
        {
            water.Property(w => w.AmountInMilliliters).HasColumnName("WaterIntakeMilliliters");
        });

        builder.Property(d => d.TeacherNote).HasMaxLength(1000);
    }
}
