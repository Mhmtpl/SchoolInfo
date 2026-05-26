using System.Threading;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Veritabanı işlemlerini UnitOfWork kullanmadan gerçekleştirmek için AppDbContext arayüzü.
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
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.Newsletter> Newsletters { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.NewsletterSection> NewsletterSections { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.ActivityTemplate> ActivityTemplates { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.DailySummary> DailySummaries { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.WeeklyMealPlan> WeeklyMealPlans { get; }
    Microsoft.EntityFrameworkCore.DbSet<SchoolInfo.Domain.Entities.ClassroomWeeklySchedule> ClassroomWeeklySchedules { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
