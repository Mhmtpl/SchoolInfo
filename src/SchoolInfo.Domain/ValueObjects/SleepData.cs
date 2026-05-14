using System;
using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.ValueObjects;

/// <summary>
/// Uyku verisini temsil eden deÄŸer nesnesi (Value Object).
/// </summary>
/// <param name="Status">Uyku durumu.</param>
/// <param name="StartTime">Uyku baÅŸlangÄ±Ã§ zamanÄ±.</param>
/// <param name="EndTime">Uyku bitiÅŸ zamanÄ±.</param>
public record SleepData(SleepStatus Status, DateTime? StartTime, DateTime? EndTime);
