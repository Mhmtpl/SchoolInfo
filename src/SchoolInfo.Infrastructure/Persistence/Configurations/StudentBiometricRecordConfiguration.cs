using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// StudentBiometricRecord entity'si için veritabanı yapılandırması.
/// </summary>
public class StudentBiometricRecordConfiguration : IEntityTypeConfiguration<StudentBiometricRecord>
{
    public void Configure(EntityTypeBuilder<StudentBiometricRecord> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.HeartRate).IsRequired(false);
        builder.Property(s => s.SpO2).IsRequired(false);
        builder.Property(s => s.BodyTemperature).IsRequired(false);
        builder.Property(s => s.RecordedAt).IsRequired();

        // Relationship mapping
        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
