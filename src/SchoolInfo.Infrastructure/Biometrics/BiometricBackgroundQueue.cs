using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Infrastructure.Biometrics;

/// <summary>
/// System.Threading.Channels tabanlı asenkron bellek içi biyometrik veri kuyruğu.
/// </summary>
public class BiometricBackgroundQueue : IBiometricBackgroundQueue
{
    private readonly Channel<BiometricQueueItem> _queue;

    public BiometricBackgroundQueue()
    {
        // En fazla 5000 kaydı bellekte tutacak şekilde sınırlandırıyoruz (OOM önleme).
        var options = new BoundedChannelOptions(5000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleWriter = false,
            SingleReader = true
        };
        _queue = Channel.CreateBounded<BiometricQueueItem>(options);
    }

    public async ValueTask QueueBiometricRecordAsync(BiometricQueueItem record)
    {
        await _queue.Writer.WriteAsync(record);
    }

    public async ValueTask<BiometricQueueItem> DequeueBiometricRecordAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
