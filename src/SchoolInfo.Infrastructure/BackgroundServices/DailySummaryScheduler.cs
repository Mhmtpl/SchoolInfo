using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;
using SchoolInfo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SchoolInfo.Infrastructure.BackgroundServices;

/// <summary>
/// Her gün 17:00'de çalışarak öğrencilerin günlük günlüğünün hazır olduğunu bildiren zamanlanmış arka plan servisi.
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
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var today = DateTime.UtcNow.AddHours(3).Date;
        
        // Bugün günlük kaydı veya devamsızlık durumu girilmiş öğrencileri getirelim
        var activeStudentIds = await dbContext.DailyRecords
            .Where(r => r.Date == today && !r.IsDeleted)
            .Select(r => r.StudentId)
            .ToListAsync(cancellationToken);

        foreach (var studentId in activeStudentIds)
        {
            try
            {
                // Maliyeti sıfır tutmak için Gemini API'yi burada çağırmıyoruz.
                // Sadece veliye bildirim atıyoruz. Veli sayfayı açtığında AI özeti anında (on-demand) üretilecektir.
                await notificationService.SendNotificationAsync(
                    studentId, 
                    "Bugünün günlüğü hazır", 
                    "Çocuğunuzun bugünkü okul günlüğü hazır! Yapay zeka özetini okumak için tıklayın.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Öğrenci {StudentId} için hazır bildirimi gönderilirken hata meydana geldi.", studentId);
            }
        }
    }
}
