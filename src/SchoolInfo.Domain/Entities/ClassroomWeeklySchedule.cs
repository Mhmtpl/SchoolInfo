using System;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Sınıfın sabit haftalık ders programı şablonu. (Pazartesi-Cuma)
/// </summary>
public class ClassroomWeeklySchedule : BaseEntity
{
    public Guid ClassroomId { get; private set; }
    public Guid SchoolId { get; private set; } // Hangi okula ait olduğunu takip için
    public int DayOfWeek { get; private set; } // 1: Pazartesi, 2: Salı, 3: Çarşamba, 4: Perşembe, 5: Cuma
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public ActivityType Type { get; private set; }

    public ClassroomWeeklySchedule(Guid classroomId, Guid schoolId, int dayOfWeek, string title, string description, TimeSpan startTime, TimeSpan endTime, ActivityType type)
    {
        ClassroomId = classroomId;
        SchoolId = schoolId;
        DayOfWeek = dayOfWeek;
        Title = title;
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        Type = type;
    }

    public void UpdateDetails(int dayOfWeek, string title, string description, TimeSpan startTime, TimeSpan endTime, ActivityType type)
    {
        DayOfWeek = dayOfWeek;
        Title = title;
        Description = description;
        StartTime = startTime;
        EndTime = endTime;
        Type = type;
        UpdateTimestamp();
    }
}
