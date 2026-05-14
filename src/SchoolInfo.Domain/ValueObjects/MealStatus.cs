using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.ValueObjects;

/// <summary>
/// Öğün durumunu temsil eden değer nesnesi (Value Object).
/// </summary>
/// <param name="Type">Öğünün tüketim tipi.</param>
/// <param name="Description">Öğün hakkında ek açıklama (isteğe bağlı).</param>
public record MealStatus(MealStatusType Type, string? Description);
