using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// GÃ¼nlÃ¼k Ã¶zetlere eriÅŸimi saÄŸlayan repository sÄ±nÄ±fÄ±.
/// </summary>
public class DailySummaryRepository : BaseRepository<DailySummary>, IDailySummaryRepository
{
    public DailySummaryRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<DailySummary?> GetByStudentAndDateAsync(Guid studentId, DateTime date)
    {
        return await DbContext.DailySummaries
            .FirstOrDefaultAsync(d => d.StudentId == studentId && d.Date.Date == date.Date);
    }
}
