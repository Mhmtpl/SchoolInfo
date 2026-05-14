using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Aktivitelere erişimi sağlayan repository sınıfı.
/// </summary>
public class ActivityRepository : BaseRepository<Activity>, IActivityRepository
{
    public ActivityRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Activity>> GetByClassroomIdAsync(Guid classroomId, DateTime date)
    {
        return await DbContext.Activities
            .Where(a => a.ClassroomId == classroomId && a.ActivityDate.Date == date.Date)
            .ToListAsync();
    }
}
