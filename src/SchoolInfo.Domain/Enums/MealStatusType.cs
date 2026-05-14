namespace SchoolInfo.Domain.Enums;

/// <summary>
/// Öğün tüketim durumlarını belirtir.
/// </summary>
public enum MealStatusType
{
    /// <summary>
    /// Hiç yemedi.
    /// </summary>
    None = 0,

    /// <summary>
    /// Az yedi.
    /// </summary>
    Little = 1,

    /// <summary>
    /// Yarısını yedi.
    /// </summary>
    Half = 2,

    /// <summary>
    /// Hepsini yedi.
    /// </summary>
    All = 3
}
