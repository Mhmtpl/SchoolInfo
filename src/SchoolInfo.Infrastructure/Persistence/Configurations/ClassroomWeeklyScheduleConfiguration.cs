using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

public class ClassroomWeeklyScheduleConfiguration : IEntityTypeConfiguration<ClassroomWeeklySchedule>
{
    public void Configure(EntityTypeBuilder<ClassroomWeeklySchedule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
    }
}
