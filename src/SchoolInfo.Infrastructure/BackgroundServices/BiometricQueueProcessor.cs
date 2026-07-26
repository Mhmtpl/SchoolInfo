using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.BackgroundServices;

public class BiometricDataPoint
{
    [JsonPropertyName("t")]
    public string Time { get; set; } = string.Empty; // HH:mm:ss

    [JsonPropertyName("hr")]
    public int? HeartRate { get; set; }

    [JsonPropertyName("sp")]
    public double? SpO2 { get; set; }

    [JsonPropertyName("temp")]
    public double? BodyTemperature { get; set; }
}

/// <summary>
/// Biyometrik veri kuyruğunu arka planda asenkron olarak tüketip günlük JSONB formatında veritabanına kaydeden
/// ve SignalR üzerinden anlık gecikmesiz yayınlayan arka plan servisi.
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
        _logger.LogInformation("Biyometrik veri kuyruk işleyici başlatıldı (Günlük JSONB optimize).");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Kuyruktan veri oku
                var record = await _queue.DequeueBiometricRecordAsync(stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<IBiometricNotificationService>();

                    var localNow = DateTime.UtcNow.AddHours(3);
                    var todayDate = localNow.Date;

                    // 1. Öğrencinin bugünkü günlük biyometrik kaydını ara
                    var dailyRecord = await dbContext.StudentBiometricRecords
                        .FirstOrDefaultAsync(r => r.StudentId == record.StudentId && 
                                                 r.Date == todayDate && 
                                                 !r.IsDeleted, 
                                             stoppingToken);

                    if (dailyRecord == null)
                    {
                        // İlk kayıt: Günlük satırı oluştur
                        dailyRecord = new StudentBiometricRecord(record.StudentId, todayDate, record.SchoolId);

                        var points = new List<BiometricDataPoint>
                        {
                            new()
                            {
                                Time = localNow.ToString("HH:mm:ss"),
                                HeartRate = record.HeartRate,
                                SpO2 = record.SpO2,
                                BodyTemperature = record.BodyTemperature
                            }
                        };

                        var json = JsonSerializer.Serialize(points);
                        dailyRecord.UpdateData(json, record.HeartRate, record.SpO2, record.BodyTemperature);

                        await dbContext.StudentBiometricRecords.AddAsync(dailyRecord, stoppingToken);
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                    else
                    {
                        // Veritabanına yazma sıklığını kontrol et: Son DB yazmasından bu yana en az 1 dakika geçmiş mi?
                        if (DateTime.UtcNow - dailyRecord.RecordedAt >= TimeSpan.FromMinutes(1))
                        {
                            var points = JsonSerializer.Deserialize<List<BiometricDataPoint>>(dailyRecord.DataJson) 
                                         ?? new List<BiometricDataPoint>();

                            points.Add(new BiometricDataPoint
                            {
                                Time = localNow.ToString("HH:mm:ss"),
                                HeartRate = record.HeartRate,
                                SpO2 = record.SpO2,
                                BodyTemperature = record.BodyTemperature
                            });

                            // Günlük ortalamaları yeniden hesapla
                            int? avgHeartRate = points.Any(p => p.HeartRate.HasValue) 
                                ? (int)Math.Round(points.Where(p => p.HeartRate.HasValue).Average(p => p.HeartRate!.Value)) 
                                : (int?)null;

                            double? avgSpO2 = points.Any(p => p.SpO2.HasValue) 
                                ? Math.Round(points.Where(p => p.SpO2.HasValue).Average(p => p.SpO2!.Value), 1) 
                                : (double?)null;

                            double? avgTemp = points.Any(p => p.BodyTemperature.HasValue) 
                                ? Math.Round(points.Where(p => p.BodyTemperature.HasValue).Average(p => p.BodyTemperature!.Value), 1) 
                                : (double?)null;

                            var json = JsonSerializer.Serialize(points);
                            dailyRecord.UpdateData(json, avgHeartRate, avgSpO2, avgTemp);

                            await dbContext.SaveChangesAsync(stoppingToken);
                        }
                    }

                    // 2. Her durumda canlı SignalR bildirimini anlık ve gecikmesiz olarak gönder
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
                // Kapatılıyor
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Biyometrik veri işlenirken hata oluştu.");
                await Task.Delay(2000, stoppingToken);
            }
        }

        _logger.LogInformation("Biyometrik veri kuyruk işleyici durduruldu.");
    }
}
