using SchoolInfo.Domain.Enums;

namespace SchoolInfo.Domain.ValueObjects;

/// <summary>
/// Ã–ÄŸÃ¼n durumunu temsil eden deÄŸer nesnesi (Value Object).
/// </summary>
/// <param name="Type">Ã–ÄŸÃ¼nÃ¼n tÃ¼ketim tipi.</param>
/// <param name="Description">Ã–ÄŸÃ¼n hakkÄ±nda ek aÃ§Ä±klama (isteÄŸe baÄŸlÄ±).</param>
public record MealStatus(MealStatusType Type, string? Description);
