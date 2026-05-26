using System;
using System.Collections.Generic;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.Entities;

public class Newsletter : BaseEntity
{
    public string Title { get; private set; }
    public string Content { get; private set; }
    public string ImageUrl { get; private set; }
    public string? WeekName { get; private set; }
    public Guid ClassroomId { get; private set; }
    public NewsletterStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    private readonly List<NewsletterSection> _sections = new();
    public IReadOnlyCollection<NewsletterSection> Sections => _sections.AsReadOnly();

    public Newsletter(string title, string content, string imageUrl, string? weekName, Guid classroomId)
    {
        Title = title;
        Content = content;
        ImageUrl = imageUrl;
        WeekName = weekName;
        ClassroomId = classroomId;
        Status = NewsletterStatus.Draft;
    }

    public void UpdateDraft(string title, string content, string imageUrl, string? weekName)
    {
        Title = title;
        Content = content;
        ImageUrl = imageUrl;
        WeekName = weekName;
        UpdateTimestamp();
    }

    public void AddSection(string subject, string thisWeekSummary, string nextWeekTopic, string instructorName)
    {
        _sections.Add(new NewsletterSection(Id, subject, thisWeekSummary, nextWeekTopic, instructorName));
        UpdateTimestamp();
    }

    public void ClearSections()
    {
        _sections.Clear();
        UpdateTimestamp();
    }

    public void Publish()
    {
        Status = NewsletterStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }
}
