using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

/// <summary>
/// Ã–ÄŸÃ¼n kayÄ±tlarÄ±na eriÅŸim iÃ§in repository arayÃ¼zÃ¼.
/// </summary>
public interface IMealRecordRepository
{
    Task<MealRecord?> GetByIdAsync(Guid id);
    Task<IEnumerable<MealRecord>> GetByDailyRecordIdAsync(Guid dailyRecordId);
    Task AddAsync(MealRecord mealRecord);
    Task UpdateAsync(MealRecord mealRecord);
    Task DeleteAsync(Guid id);
}
