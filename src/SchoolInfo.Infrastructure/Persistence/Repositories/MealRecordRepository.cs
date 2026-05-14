using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Ã–ÄŸÃ¼n kayÄ±tlarÄ±na eriÅŸimi saÄŸlayan repository sÄ±nÄ±fÄ±.
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
