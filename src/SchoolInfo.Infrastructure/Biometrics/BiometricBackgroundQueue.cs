using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.Infrastructure.Biometrics;

/// <summary>
/// System.Threading.Channels tabanlı asenkron bellek içi biyometrik veri kuyruğu.
/// </summary>
public class BiometricBackgroundQueue : IBiometricBackgroundQueue
{
    private readonly Channel<StudentBiometricRecord> _queue;

    public BiometricBackgroundQueue()
    {
        // En fazla 5000 kaydı bellekte tutacak şekilde sınırlandırıyoruz (OOM önleme).
        // Kuyruk dolarsa yeni gelen veri yazılırken bekler veya drop edilir.
        var options = new BoundedChannelOptions(5000)
        {
            FullMode = BoundedChannelFullMode.DropOldest, // Kuyruk dolarsa en eski okunmamış veriyi düşür
            SingleWriter = false,
            SingleReader = true
        };
        _queue = Channel.CreateBounded<StudentBiometricRecord>(options);
    }

    public async ValueTask QueueBiometricRecordAsync(StudentBiometricRecord record)
    {
        await _queue.Writer.WriteAsync(record);
    }

    public async ValueTask<StudentBiometricRecord> DequeueBiometricRecordAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
