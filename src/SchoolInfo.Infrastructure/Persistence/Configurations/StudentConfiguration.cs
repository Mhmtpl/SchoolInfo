using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// Student entity'si iÃ§in veritabanÄ± yapÄ±landÄ±rmasÄ±.
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.DateOfBirth).IsRequired();

        builder.HasMany(s => s.Parents)
            .WithMany(u => u.Students)
            .UsingEntity(j => j.ToTable("StudentParents"));
    }
}
