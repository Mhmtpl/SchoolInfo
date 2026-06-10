using System;

namespace SchoolInfo.Application.Features.DailyRecords.Queries.GetStudentDailyRecord;

/// <summary>
/// GÃ¼nlÃ¼k kayÄ±t verisini dÃ¶ndÃ¼ren DTO.
/// </summary>
public record DailyRecordDto(
    Guid Id,
    Guid StudentId,
    DateTime Date,
    string SleepStatus,
    int WaterIntake,
    string? TeacherNote);
