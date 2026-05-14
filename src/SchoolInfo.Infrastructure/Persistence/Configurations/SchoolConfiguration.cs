using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// School entity'si iÃ§in veritabanÄ± yapÄ±landÄ±rmasÄ±.
/// </summary>
public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);

        builder.HasMany(s => s.Classrooms)
               .WithOne()
               .HasForeignKey(c => c.SchoolId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
