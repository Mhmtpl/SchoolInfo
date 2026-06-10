using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;
using SchoolInfo.Infrastructure.Persistence;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

public class MedicationRecordRepository : BaseRepository<MedicationRecord>, IMedicationRecordRepository
{
    public MedicationRecordRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<MedicationRecord>> GetByStudentAndDateAsync(Guid studentId, DateTime date)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await DbContext.Set<MedicationRecord>()
            .Where(m => m.StudentId == studentId && m.AdministrationTime != null)
            .Where(m => DbContext.DailyRecords.Any(d => d.Id == m.DailyRecordId && d.StudentId == studentId && d.Date == utcDate && !d.IsDeleted))
            .ToListAsync();
    }
}
