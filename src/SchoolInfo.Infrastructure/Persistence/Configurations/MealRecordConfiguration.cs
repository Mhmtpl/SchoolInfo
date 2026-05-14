using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// MealRecord entity'si için veritabanı yapılandırması.
/// </summary>
public class MealRecordConfiguration : IEntityTypeConfiguration<MealRecord>
{
    public void Configure(EntityTypeBuilder<MealRecord> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MealName).IsRequired().HasMaxLength(100);

        // Owned Entity: MealStatus
        builder.OwnsOne(m => m.Status, status =>
        {
            status.Property(s => s.Type).HasColumnName("StatusType");
            status.Property(s => s.Description).HasColumnName("StatusDescription").HasMaxLength(500);
        });
    }
}
