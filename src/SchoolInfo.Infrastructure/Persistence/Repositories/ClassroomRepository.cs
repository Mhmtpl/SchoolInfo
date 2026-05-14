using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Sınıf verilerine erişimi sağlayan repository sınıfı.
/// Her sorguda schoolId ve IsDeleted filtresi uygulanır.
/// </summary>
public class ClassroomRepository : BaseRepository<Classroom>, IClassroomRepository
{
    public ClassroomRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Belirtilen okula ait tek bir sınıfı getirir.
    /// </summary>
    public async Task<Classroom?> GetByIdAsync(Guid id, Guid schoolId)
    {
        return await DbContext.Classrooms
            .FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == schoolId && !c.IsDeleted);
    }

    /// <summary>
    /// Okula ait tüm sınıfları getirir (silinmişler hariç).
    /// </summary>
    public async Task<List<Classroom>> GetAllBySchoolAsync(Guid schoolId)
    {
        return await DbContext.Classrooms
            .Where(c => c.SchoolId == schoolId && !c.IsDeleted)
            .ToListAsync();
    }

    /// <summary>
    /// Öğretmene atanmış sınıfları getirir.
    /// </summary>
    public async Task<List<Classroom>> GetByTeacherIdAsync(Guid teacherId, Guid schoolId)
    {
        return await DbContext.Classrooms
            .Include(c => c.Teachers)
            .Where(c => c.SchoolId == schoolId && !c.IsDeleted && c.Teachers.Any(t => t.Id == teacherId))
            .ToListAsync();
    }

    /// <summary>
    /// Yeni sınıf ekler.
    /// </summary>
    public async Task AddAsync(Classroom classroom)
    {
        await DbContext.Classrooms.AddAsync(classroom);
    }

    /// <summary>
    /// Mevcut sınıfı günceller.
    /// </summary>
    public Task UpdateAsync(Classroom classroom)
    {
        DbContext.Classrooms.Update(classroom);
        return Task.CompletedTask;
    }
}
