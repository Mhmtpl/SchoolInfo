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
/// Uygulamanın veritabanı bağlamı ve Unit Of Work implementasyonu.
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

        // 2. Event'leri temizle (Sonsuz döngüyü önlemek için)
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        // 3. Veritabanına kaydet
        var result = await base.SaveChangesAsync(cancellationToken);

        // 4. MediatR üzerinden event'leri fırlat
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}
