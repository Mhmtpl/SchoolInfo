using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

public class ActivityTemplateConfiguration : IEntityTypeConfiguration<ActivityTemplate>
{
    public void Configure(EntityTypeBuilder<ActivityTemplate> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(2000);
        builder.Property(a => a.Category).HasMaxLength(100);
        builder.Property(a => a.RequiredMaterials).HasMaxLength(1000);
        builder.Property(a => a.AgeGroup).HasMaxLength(50);
    }
}
