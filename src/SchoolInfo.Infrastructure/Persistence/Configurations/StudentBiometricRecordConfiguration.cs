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
        
        builder.Property(s => s.Date).IsRequired();
        builder.Property(s => s.DataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(s => s.AverageHeartRate).IsRequired(false);
        builder.Property(s => s.AverageSpO2).IsRequired(false);
        builder.Property(s => s.AverageBodyTemperature).IsRequired(false);
        builder.Property(s => s.RecordedAt).IsRequired();

        // Bir öğrencinin aynı gün için tek bir satırı olmasını garanti eden benzersiz indeks (Unique Index)
        builder.HasIndex(s => new { s.StudentId, s.Date }).IsUnique();

        // Relationship mapping
        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
