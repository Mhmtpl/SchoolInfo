using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Persistence;

/// <summary>
/// UygulamanÄ±n veritabanÄ± baÄŸlamÄ± ve Unit Of Work implementasyonu.
/// </summary>
public class AppDbContext : DbContext, IAppDbContext
{
    private readonly IMediator _mediator;

    public DbSet<School> Schools => Set<School>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DailyRecord> DailyRecords => Set<DailyRecord>();
    public DbSet<MealRecord> MealRecords => Set<MealRecord>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<DailySummary> DailySummaries => Set<DailySummary>();

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        // Global Query Filters for Soft Delete
        modelBuilder.Entity<School>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Classroom>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DailyRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MealRecord>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Activity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DailySummary>().HasQueryFilter(e => !e.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Domain Event'leri al
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // 2. Event'leri temizle (Sonsuz dÃ¶ngÃ¼yÃ¼ Ã¶nlemek iÃ§in)
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        // 3. VeritabanÄ±na kaydet
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. MediatR Ã¼zerinden event'leri fÄ±rlat
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
