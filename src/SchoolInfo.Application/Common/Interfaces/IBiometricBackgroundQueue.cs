using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Common.Interfaces;

public record BiometricQueueItem(
    Guid StudentId,
    Guid SchoolId,
    int? HeartRate,
    double? SpO2,
    double? BodyTemperature,
    DateTime RecordedAt
);

/// <summary>
/// Biyometrik verileri asenkron olarak bellek içi kuyruğa alan ve tüketen servis arayüzü.
/// </summary>
public interface IBiometricBackgroundQueue
{
    ValueTask QueueBiometricRecordAsync(BiometricQueueItem record);
    ValueTask<BiometricQueueItem> DequeueBiometricRecordAsync(CancellationToken cancellationToken);
}
