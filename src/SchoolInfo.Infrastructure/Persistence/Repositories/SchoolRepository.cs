using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Okul verilerine erişimi sağlayan repository sınıfı.
/// </summary>
public class SchoolRepository : BaseRepository<School>, ISchoolRepository
{
    public SchoolRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Belirtilen ID'ye sahip okulu getirir.
    /// </summary>
    public async Task<School?> GetByIdAsync(Guid id)
    {
        return await DbContext.Schools
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    /// <summary>
    /// Sistemdeki tüm aktif okulları getirir.
    /// </summary>
    public async Task<List<School>> GetAllAsync()
    {
        return await DbContext.Schools
            .Where(s => !s.IsDeleted)
            .ToListAsync();
    }

    /// <summary>
    /// Yeni okul ekler.
    /// </summary>
    public async Task AddAsync(School school)
    {
        await DbContext.Schools.AddAsync(school);
    }
}
