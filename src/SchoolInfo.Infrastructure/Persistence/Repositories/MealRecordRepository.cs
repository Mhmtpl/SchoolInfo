using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Öğün kayıtlarına erişimi sağlayan repository sınıfı.
/// </summary>
public class MealRecordRepository : BaseRepository<MealRecord>, IMealRecordRepository
{
    public MealRecordRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<MealRecord>> GetByDailyRecordIdAsync(Guid dailyRecordId)
    {
        return await DbContext.MealRecords
            .Where(m => m.DailyRecordId == dailyRecordId)
            .ToListAsync();
    }
}
