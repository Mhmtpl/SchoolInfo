using System.Threading;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// VeritabanÄ± iÅŸlemlerini UnitOfWork kullanmadan gerÃ§ekleÅŸtirmek iÃ§in AppDbContext arayÃ¼zÃ¼.
/// </summary>
public interface IAppDbContext
{
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.School> Schools { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.Classroom> Classrooms { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.Student> Students { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.User> Users { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.DailyRecord> DailyRecords { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.MealRecord> MealRecords { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.Activity> Activities { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.DailySummary> DailySummaries { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
