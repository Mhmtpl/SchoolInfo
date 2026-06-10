using System;
using MediatR;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.UpdateDailyRecord;

/// <summary>
/// GÃ¼nlÃ¼k kaydÄ± gÃ¼ncelleme isteÄŸi.
/// </summary>
public record UpdateDailyRecordCommand(
    Guid DailyRecordId, 
    SleepStatus SleepStatus, 
    DateTime? SleepStartTime, 
    DateTime? SleepEndTime, 
    int WaterAmountInMilliliters, 
    string? TeacherNote,
    bool? IsAbsent = null) : IRequest;
