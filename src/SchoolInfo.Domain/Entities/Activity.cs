using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// SÄ±nÄ±f veya Ã¶ÄŸrenci aktivitesi.
/// </summary>
public class Activity : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime ActivityDate { get; private set; }
    public Guid ClassroomId { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Activity(string title, string description, DateTime activityDate, Guid classroomId)
    {
        Title = title;
        Description = description;
        ActivityDate = activityDate;
        ClassroomId = classroomId;
    }

    /// <summary>
    /// Aktivite bilgilerini gÃ¼nceller.
    /// </summary>
    public void UpdateDetails(string title, string description, DateTime activityDate)
    {
        Title = title;
        Description = description;
        ActivityDate = activityDate;
        UpdateTimestamp();
    }

    /// <summary>
    /// Aktiviteyi tamamlandÄ± olarak iÅŸaretler.
    /// </summary>
    public void Complete()
    {
        CompletedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }
}
