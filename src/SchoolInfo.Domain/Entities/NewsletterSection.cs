using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

public class NewsletterSection : BaseEntity
{
    public Guid NewsletterId { get; private set; }
    public string Subject { get; private set; }
    public string ThisWeekSummary { get; private set; }
    public string NextWeekTopic { get; private set; }
    public string InstructorName { get; private set; }

    // Navigation property
    public virtual Newsletter Newsletter { get; private set; } = null!;

    public NewsletterSection(Guid newsletterId, string subject, string thisWeekSummary, string nextWeekTopic, string instructorName)
    {
        NewsletterId = newsletterId;
        Subject = subject;
        ThisWeekSummary = thisWeekSummary;
        NextWeekTopic = nextWeekTopic;
        InstructorName = instructorName;
    }

    // For EF Core
    protected NewsletterSection() { }

    public void Update(string thisWeekSummary, string nextWeekTopic, string instructorName)
    {
        ThisWeekSummary = thisWeekSummary;
        NextWeekTopic = nextWeekTopic;
        InstructorName = instructorName;
        UpdateTimestamp();
    }
}
