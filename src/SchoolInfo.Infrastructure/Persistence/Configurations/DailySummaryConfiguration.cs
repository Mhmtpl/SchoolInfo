using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// DailySummary entity'si için veritabanı yapılandırması.
/// </summary>
public class DailySummaryConfiguration : IEntityTypeConfiguration<DailySummary>
{
    public void Configure(EntityTypeBuilder<DailySummary> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Date).IsRequired();
        builder.Property(d => d.Content).IsRequired().HasMaxLength(4000);
        builder.Property(d => d.IsReadByParent).IsRequired().HasDefaultValue(false);
    }
}
