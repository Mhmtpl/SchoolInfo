namespace SchoolInfo.Domain.Enums;

/// <summary>
/// Uyku durumunu belirtir.
/// </summary>
public enum SleepStatus
{
    /// <summary>
    /// Uyumadı.
    /// </summary>
    DidNotSleep = 0,

    /// <summary>
    /// Az uyudu.
    /// </summary>
    SleptLittle = 1,

    /// <summary>
    /// Normal uyudu.
    /// </summary>
    SleptWell = 2
}
