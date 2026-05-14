using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Tüm repository'ler için temel generic sınıf.
/// </summary>
public abstract class BaseRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext DbContext;

    protected BaseRepository(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbContext.Set<T>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public virtual async Task AddAsync(T entity)
    {
        await DbContext.Set<T>().AddAsync(entity);
    }

    public virtual Task UpdateAsync(T entity)
    {
        DbContext.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            DbContext.Set<T>().Remove(entity);
        }
    }
}
