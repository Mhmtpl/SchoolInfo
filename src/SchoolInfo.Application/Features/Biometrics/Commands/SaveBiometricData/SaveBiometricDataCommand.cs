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
        _logger.LogWarning($"[BIOMETRIC] Received request: MacAddress='{request.MacAddress}', HeartRate='{request.HeartRate}'");

        if (string.IsNullOrWhiteSpace(request.MacAddress))
        {
            _logger.LogWarning("[BIOMETRIC] MacAddress is null or empty!");
            return false;
        }

        // MAC adresine sahip öğrenciyi bul (büyük/küçük harf duyarsız eşleştirme için Normalize edebiliriz)
        var targetMac = request.MacAddress.Trim().Replace("-", ":").ToUpperInvariant();

        try
        {
            var allStudents = await _dbContext.Students
                .Select(s => new { s.Id, s.FirstName, s.LastName, s.SmartBandMacAddress, s.IsDeleted })
                .ToListAsync(cancellationToken);
            _logger.LogWarning($"[BIOMETRIC] Total active/inactive students in database: {allStudents.Count}");
            foreach (var s in allStudents)
            {
                _logger.LogWarning($"[BIOMETRIC] DB Student: {s.FirstName} {s.LastName} | ID: {s.Id} | MAC: '{s.SmartBandMacAddress}' | IsDeleted: {s.IsDeleted}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"[BIOMETRIC] DB Student debug fetch error: {ex.Message}");
        }

        var students = await _dbContext.Students
            .Where(s => s.SmartBandMacAddress != null && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        _logger.LogWarning($"[BIOMETRIC] Found {students.Count} students in DB with non-null SmartBandMacAddress");

        var student = students.FirstOrDefault(s => 
            s.SmartBandMacAddress!.Trim().Replace("-", ":").ToUpperInvariant() == targetMac);

        if (student == null)
        {
            _logger.LogWarning($"[BIOMETRIC] Student not found in-memory for targetMac: '{targetMac}'");
            return false;
        }

        // Biyometrik kaydı oluştur
        var biometricRecord = new StudentBiometricRecord(
            student.Id,
            request.HeartRate,
            request.SpO2,
            request.BodyTemperature,
            DateTime.UtcNow,
            student.SchoolId
        );

        // Kuyruğa ekle
        await _backgroundQueue.QueueBiometricRecordAsync(biometricRecord);
        return true;
    }
}
