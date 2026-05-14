using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Domain.Interfaces;

/// <summary>
/// GÃ¼nlÃ¼k kayÄ±tlara eriÅŸim iÃ§in repository arayÃ¼zÃ¼.
/// </summary>
public interface IDailyRecordRepository
{
    Task<DailyRecord?> GetByIdAsync(Guid id);
    Task<DailyRecord?> GetByStudentAndDateAsync(Guid studentId, DateTime date);
    Task AddAsync(DailyRecord dailyRecord);
    Task UpdateAsync(DailyRecord dailyRecord);
}
