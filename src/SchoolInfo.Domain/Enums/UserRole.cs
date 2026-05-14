namespace SchoolInfo.Domain.Enums;

/// <summary>
/// Sistemdeki kullanıcı rollerini belirtir.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Yönetici rolü.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Öğretmen rolü.
    /// </summary>
    Teacher = 2,

    /// <summary>
    /// Veli rolü.
    /// </summary>
    Parent = 3
}
