using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

/// <summary>
/// Günlük özetlere erişim için repository arayüzü.
/// </summary>
public interface IDailySummaryRepository
{
    Task<DailySummary?> GetByIdAsync(Guid id);
    Task<DailySummary?> GetByStudentAndDateAsync(Guid studentId, DateTime date);
    Task AddAsync(DailySummary dailySummary);
    Task UpdateAsync(DailySummary dailySummary);
}
