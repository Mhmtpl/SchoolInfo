using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

/// <summary>
/// Classroom entity'si için veritabanı yapılandırması.
/// </summary>
public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
{
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(c => c.Students)
               .WithOne()
               .HasForeignKey(s => s.ClassroomId)
               .OnDelete(DeleteBehavior.Cascade);

        // Birden fazla öğretmen atanabilir (many-to-many)
        builder.HasMany(c => c.Teachers)
               .WithMany(u => u.Classrooms)
               .UsingEntity(j => j.ToTable("ClassroomTeachers"));
    }
}
