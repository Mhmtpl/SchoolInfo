using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// GÃ¼nlÃ¼k kayÄ±tlara eriÅŸimi saÄŸlayan repository sÄ±nÄ±fÄ±.
/// </summary>
public class DailyRecordRepository : BaseRepository<DailyRecord>, IDailyRecordRepository
{
    public DailyRecordRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<DailyRecord?> GetByStudentAndDateAsync(Guid studentId, DateTime date)
    {
        return await DbContext.DailyRecords
            .FirstOrDefaultAsync(d => d.StudentId == studentId && d.Date.Date == date.Date);
    }
}
