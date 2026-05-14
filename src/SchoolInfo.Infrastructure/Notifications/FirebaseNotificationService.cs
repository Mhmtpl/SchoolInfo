using System;
using System.Threading.Tasks;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Infrastructure.Notifications;

/// <summary>
/// Firebase kullanarak kullanıcılara anlık bildirim (Push Notification) gönderen servis.
/// </summary>
public class FirebaseNotificationService : INotificationService
{
    public Task SendNotificationAsync(Guid userId, string title, string message)
    {
        // FirebaseAdmin SDK kullanılarak bildirim gönderim mantığı burada yer alır.
        return Task.CompletedTask;
    }
}
