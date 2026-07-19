using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Infrastructure.BackgroundServices;

/// <summary>
/// Biyometrik veri kuyruğunu arka planda asenkron olarak tüketip veritabanına kaydeden
/// ve SignalR üzerinden anlık yayınlayan arka plan servisi.
/// </summary>
public class BiometricQueueProcessor : BackgroundService
{
    private readonly IBiometricBackgroundQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BiometricQueueProcessor> _logger;

    public BiometricQueueProcessor(
        IBiometricBackgroundQueue queue, 
        IServiceProvider serviceProvider, 
        ILogger<BiometricQueueProcessor> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Biyometrik veri kuyruk işleyici başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Kuyruktan veri oku (yeni veri gelene kadar asenkron bekler)
                var record = await _queue.DequeueBiometricRecordAsync(stoppingToken);

                // Scoped servisleri çözmek için yeni bir scope oluşturuyoruz
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<IBiometricNotificationService>();

                    // 1. Veritabanına kaydet
                    await dbContext.StudentBiometricRecords.AddAsync(record, stoppingToken);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    // 2. Gerçek zamanlı SignalR yayını yap
                    await notificationService.SendBiometricUpdateAsync(
                        record.SchoolId,
                        record.StudentId,
                        record.HeartRate,
                        record.SpO2,
                        record.BodyTemperature,
                        record.RecordedAt
                    );
                }
            }
            catch (OperationCanceledException)
            {
                // Uygulama kapatılıyor
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Biyometrik veri işlenirken hata oluştu.");
                // Hata durumunda CPU'yu tüketmemek için kısa bir süre bekliyoruz
                await Task.Delay(2000, stoppingToken);
            }
        }

        _logger.LogInformation("Biyometrik veri kuyruk işleyici durduruldu.");
    }
}
