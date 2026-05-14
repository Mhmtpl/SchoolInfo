using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;
using SchoolInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SchoolInfo.Infrastructure.BackgroundServices;

/// <summary>
/// Her gün 17:00'de çalışarak öğrencilerin günlük özetlerini oluşturan zamanlanmış arka plan servisi.
/// </summary>
public class DailySummaryScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailySummaryScheduler> _logger;

    public DailySummaryScheduler(IServiceProvider serviceProvider, ILogger<DailySummaryScheduler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var targetTime = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0);

            if (now > targetTime)
            {
                targetTime = targetTime.AddDays(1);
            }

            var delay = targetTime - now;
            _logger.LogInformation("DailySummaryScheduler {TargetTime} saatinde çalışmak üzere bekliyor.", targetTime);

            try
            {
                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("DailySummaryScheduler çalışmaya başladı.");
                await ProcessSummariesAsync(stoppingToken);
                _logger.LogInformation("DailySummaryScheduler işlemi tamamlandı.");
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DailySummaryScheduler döngüsünde hata oluştu.");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task ProcessSummariesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateTime.UtcNow.Date;
        
        var studentIds = await dbContext.Students.Select(s => s.Id).ToListAsync(cancellationToken);

        foreach (var studentId in studentIds)
        {
            try
            {
                var command = new GenerateDailySummaryCommand(studentId, today);
                await mediator.Send(command, cancellationToken);
            }
            catch (Exception ex)
            {
                // Bir öğrencinin hatası diğerini etkilemesin
                _logger.LogError(ex, "Öğrenci {StudentId} için günlük özet oluşturulurken hata meydana geldi.", studentId);
            }
        }
    }
}
