using System;
using MediatR;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Application.Features.DailyRecords.Commands.UpdateDailyRecord;

/// <summary>
/// Günlük kaydı güncelleme isteği.
/// </summary>
public record UpdateDailyRecordCommand(
    Guid DailyRecordId, 
    SleepStatus SleepStatus, 
    DateTime? SleepStartTime, 
    DateTime? SleepEndTime, 
    int WaterAmountInMilliliters, 
    string? TeacherNote) : IRequest;
