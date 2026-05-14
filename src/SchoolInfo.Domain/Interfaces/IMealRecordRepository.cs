using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

/// <summary>
/// Öğün kayıtlarına erişim için repository arayüzü.
/// </summary>
public interface IMealRecordRepository
{
    Task<MealRecord?> GetByIdAsync(Guid id);
    Task<IEnumerable<MealRecord>> GetByDailyRecordIdAsync(Guid dailyRecordId);
    Task AddAsync(MealRecord mealRecord);
    Task UpdateAsync(MealRecord mealRecord);
    Task DeleteAsync(Guid id);
}
