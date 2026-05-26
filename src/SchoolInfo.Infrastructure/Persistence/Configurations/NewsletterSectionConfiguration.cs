using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence.Configurations;

public class NewsletterSectionConfiguration : IEntityTypeConfiguration<NewsletterSection>
{
    public void Configure(EntityTypeBuilder<NewsletterSection> builder)
    {
        builder.ToTable("NewsletterSections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ThisWeekSummary)
            .IsRequired();

        builder.Property(x => x.NextWeekTopic)
            .IsRequired();

        builder.Property(x => x.InstructorName)
            .HasMaxLength(100);
            
        builder.HasOne(x => x.Newsletter)
            .WithMany(n => n.Sections)
            .HasForeignKey(x => x.NewsletterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
