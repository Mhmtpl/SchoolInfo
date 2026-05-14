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
/// Her gÃ¼n 17:00'de Ã§alÄ±ÅŸarak Ã¶ÄŸrencilerin gÃ¼nlÃ¼k Ã¶zetlerini oluÅŸturan zamanlanmÄ±ÅŸ arka plan servisi.
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
            _logger.LogInformation("DailySummaryScheduler {TargetTime} saatinde Ã§alÄ±ÅŸmak Ã¼zere bekliyor.", targetTime);

            try
            {
                await Task.Delay(delay, stoppingToken);

                _logger.LogInformation("DailySummaryScheduler Ã§alÄ±ÅŸmaya baÅŸladÄ±.");
                await ProcessSummariesAsync(stoppingToken);
                _logger.LogInformation("DailySummaryScheduler iÅŸlemi tamamlandÄ±.");
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DailySummaryScheduler dÃ¶ngÃ¼sÃ¼nde hata oluÅŸtu.");
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
                // Bir Ã¶ÄŸrencinin hatasÄ± diÄŸerini etkilemesin
                _logger.LogError(ex, "Ã–ÄŸrenci {StudentId} iÃ§in gÃ¼nlÃ¼k Ã¶zet oluÅŸturulurken hata meydana geldi.", studentId);
            }
        }
    }
}
