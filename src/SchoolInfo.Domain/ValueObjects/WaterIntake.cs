namespace SchoolInfo.Domain.ValueObjects;

/// <summary>
/// Su tüketimi verisini temsil eden değer nesnesi (Value Object).
/// </summary>
/// <param name="AmountInMilliliters">Mililitre cinsinden tüketilen su miktarı.</param>
public record WaterIntake(int AmountInMilliliters);
