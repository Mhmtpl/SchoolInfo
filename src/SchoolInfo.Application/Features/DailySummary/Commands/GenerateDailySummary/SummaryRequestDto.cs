using System;
using System.Collections.Generic;

namespace SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

/// <summary>
/// Yapay zekaya gÃ¶nderilecek Ã¶zet verisi modeli.
/// </summary>
public record SummaryRequestDto(
    string StudentName,
    DateTime Date,
    WaterIntakeDto WaterIntake,
    SleepDto Sleep,
    int ToiletCount,
    List<MealDto> Meals,
    List<ActivityDto> Activities
);

public record WaterIntakeDto(int AmountMl, bool WasDrunk);
public record SleepDto(int Minutes, string Status);
public record MealDto(string MealName, string Status, string Note);
public record ActivityDto(string Title, string Goal, DateTime? CompletedAt);
