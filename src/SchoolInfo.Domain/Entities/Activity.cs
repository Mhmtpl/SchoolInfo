using System;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// SÄ±nÄ±f veya Ã¶ÄŸrenci aktivitesi.
/// </summary>
public class Activity : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime ActivityDate { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public ActivityType Type { get; private set; }
    public Guid ClassroomId { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Activity(string title, string description, DateTime activityDate, TimeSpan startTime, TimeSpan endTime, ActivityType type, Guid classroomId)
    {
        Title = title;
        Description = description;
        ActivityDate = activityDate;
        StartTime = startTime;
        EndTime = endTime;
        Type = type;
        ClassroomId = classroomId;
    }

    /// <summary>
    /// Aktivite bilgilerini günceller.
    /// </summary>
    public void UpdateDetails(string title, string description, DateTime activityDate, TimeSpan startTime, TimeSpan endTime, ActivityType type)
    {
        Title = title;
        Description = description;
        ActivityDate = activityDate;
        StartTime = startTime;
        EndTime = endTime;
        Type = type;
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
