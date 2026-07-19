using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.API.Hubs;

/// <summary>
/// IBiometricNotificationService arayüzünü uygulayarak SignalR Hub üzerinden
/// canlı biyometrik verileri istemcilere (veli/öğretmen) aktarır.
/// </summary>
public class BiometricNotificationService : IBiometricNotificationService
{
    private readonly IHubContext<BiometricHub> _hubContext;

    public BiometricNotificationService(IHubContext<BiometricHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendBiometricUpdateAsync(
        Guid schoolId,
        Guid studentId, 
        int? heartRate, 
        double? spO2, 
        double? bodyTemperature, 
        DateTime recordedAt)
    {
        // Öğrenciye özel grubu (Student_{studentId}) hedef alarak veriyi gönderiyoruz
        await _hubContext.Clients.Group($"Student_{studentId}").SendAsync(
            "ReceiveBiometricUpdate", 
            new
            {
                studentId,
                heartRate,
                spO2,
                bodyTemperature,
                recordedAt = recordedAt.ToString("o") // ISO-8601 string format
            });
    }
}
