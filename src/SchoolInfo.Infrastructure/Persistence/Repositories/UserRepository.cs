using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Kullanıcı verilerine erişimi sağlayan repository sınıfı.
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Belirtilen ID'ye sahip kullanıcıyı getirir.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await DbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    /// <summary>
    /// E-posta adresine göre kullanıcıyı getirir (giriş işlemleri için).
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    /// <summary>
    /// Yeni kullanıcı ekler.
    /// </summary>
    public async Task AddAsync(User user)
    {
        await DbContext.Users.AddAsync(user);
    }

    /// <summary>
    /// Mevcut kullanıcıyı günceller.
    /// </summary>
    public Task UpdateAsync(User user)
    {
        DbContext.Users.Update(user);
        return Task.CompletedTask;
    }
}
