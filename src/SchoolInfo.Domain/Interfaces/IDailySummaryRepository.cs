using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

/// <summary>
/// GÃ¼nlÃ¼k Ã¶zetlere eriÅŸim iÃ§in repository arayÃ¼zÃ¼.
/// </summary>
public interface IDailySummaryRepository
{
    Task<DailySummary?> GetByIdAsync(Guid id);
    Task<DailySummary?> GetByStudentAndDateAsync(Guid studentId, DateTime date);
    Task AddAsync(DailySummary dailySummary);
    Task UpdateAsync(DailySummary dailySummary);
}
