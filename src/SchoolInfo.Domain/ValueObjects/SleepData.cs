using System;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.ValueObjects;

/// <summary>
/// Uyku verisini temsil eden değer nesnesi (Value Object).
/// </summary>
/// <param name="Status">Uyku durumu.</param>
/// <param name="StartTime">Uyku başlangıç zamanı.</param>
/// <param name="EndTime">Uyku bitiş zamanı.</param>
public record SleepData(SleepStatus Status, DateTime? StartTime, DateTime? EndTime);
