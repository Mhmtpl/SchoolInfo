using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Features.Biometrics.Commands.SaveBiometricData;

public record SaveBiometricDataCommand(
    [property: JsonPropertyName("macAddress")] string MacAddress,
    [property: JsonPropertyName("heartRate")] int? HeartRate,
    [property: JsonPropertyName("spO2")] double? SpO2,
    [property: JsonPropertyName("bodyTemperature")] double? BodyTemperature
) : IRequest<bool>;

public class SaveBiometricDataCommandHandler : IRequestHandler<SaveBiometricDataCommand, bool>
{
    private readonly IAppDbContext _dbContext;
    private readonly IBiometricBackgroundQueue _backgroundQueue;
    private readonly ILogger<SaveBiometricDataCommandHandler> _logger;

    public SaveBiometricDataCommandHandler(
        IAppDbContext dbContext, 
        IBiometricBackgroundQueue backgroundQueue,
        ILogger<SaveBiometricDataCommandHandler> logger)
    {
        _dbContext = dbContext;
        _backgroundQueue = backgroundQueue;
        _logger = logger;
    }

    public async Task<bool> Handle(SaveBiometricDataCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[BIOMETRIC] Received request: MacAddress='{request.MacAddress}', HeartRate='{request.HeartRate}'");

        if (string.IsNullOrWhiteSpace(request.MacAddress))
        {
            _logger.LogWarning("[BIOMETRIC] MacAddress is null or empty!");
            return false;
        }

        // MAC adresine sahip öğrenciyi bul (büyük/küçük harf duyarsız eşleştirme için Normalize edebiliriz)
        var targetMac = request.MacAddress.Trim().Replace("-", ":").ToUpperInvariant();

        var students = await _dbContext.Students
            .Where(s => s.SmartBandMacAddress != null && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        var student = students.FirstOrDefault(s => 
            s.SmartBandMacAddress!.Trim().Replace("-", ":").ToUpperInvariant() == targetMac);

        if (student == null)
        {
            _logger.LogWarning($"[BIOMETRIC] Student not found in-memory for targetMac: '{targetMac}'");
            return false;
        }

        // Biyometrik kuyruk öğesini oluştur
        var queueItem = new BiometricQueueItem(
            student.Id,
            student.SchoolId,
            request.HeartRate,
            request.SpO2,
            request.BodyTemperature,
            DateTime.UtcNow
        );

        // Kuyruğa ekle
        await _backgroundQueue.QueueBiometricRecordAsync(queueItem);
        return true;
    }
}
