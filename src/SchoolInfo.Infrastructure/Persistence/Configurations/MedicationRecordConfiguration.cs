using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// MedicationRecord entity yapılandırması.
/// </summary>
public class MedicationRecordConfiguration : IEntityTypeConfiguration<MedicationRecord>
{
    public void Configure(EntityTypeBuilder<MedicationRecord> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MedicineName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Dosage).HasMaxLength(200);
        builder.Property(m => m.AdministrationTime).IsRequired().HasMaxLength(20);
        builder.Property(m => m.Note).HasMaxLength(1000);
    }
}
