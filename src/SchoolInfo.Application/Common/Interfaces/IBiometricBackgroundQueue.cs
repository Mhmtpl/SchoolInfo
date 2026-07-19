using System.Threading;
using System.Threading.Tasks;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Biyometrik verileri asenkron olarak bellek içi kuyruğa alan ve tüketen servis arayüzü.
/// </summary>
public interface IBiometricBackgroundQueue
{
    ValueTask QueueBiometricRecordAsync(StudentBiometricRecord record);
    ValueTask<StudentBiometricRecord> DequeueBiometricRecordAsync(CancellationToken cancellationToken);
}
