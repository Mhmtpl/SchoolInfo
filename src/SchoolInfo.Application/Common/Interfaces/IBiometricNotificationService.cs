using System;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Biyometrik verileri istemcilere (veli ve öğretmen) gerçek zamanlı yayınlayan servis arayüzü.
/// </summary>
public interface IBiometricNotificationService
{
    Task SendBiometricUpdateAsync(
        Guid schoolId,
        Guid studentId, 
        int? heartRate, 
        double? spO2, 
        double? bodyTemperature, 
        DateTime recordedAt);
}
