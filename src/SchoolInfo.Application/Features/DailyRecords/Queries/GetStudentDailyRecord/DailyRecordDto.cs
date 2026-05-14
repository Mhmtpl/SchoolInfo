using System;

namespace SchoolInfo.Application.Features.DailyRecords.Queries.GetStudentDailyRecord;

/// <summary>
/// Günlük kayıt verisini döndüren DTO.
/// </summary>
public record DailyRecordDto(
    Guid Id,
    Guid StudentId,
    DateTime Date,
    string SleepStatus,
    int WaterIntake,
    string? TeacherNote);
