using System;
using System.Threading.Tasks;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.Infrastructure.Notifications;

/// <summary>
/// Firebase kullanarak kullanÄ±cÄ±lara anlÄ±k bildirim (Push Notification) gÃ¶nderen servis.
/// </summary>
public class FirebaseNotificationService : INotificationService
{
    public Task SendNotificationAsync(Guid userId, string title, string message)
    {
        // FirebaseAdmin SDK kullanÄ±larak bildirim gÃ¶nderim mantÄ±ÄŸÄ± burada yer alÄ±r.
        return Task.CompletedTask;
    }
}
