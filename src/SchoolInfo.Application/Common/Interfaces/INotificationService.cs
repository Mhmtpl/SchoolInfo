using System;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Kullanıcılara bildirim gönderen servis.
/// </summary>
public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string title, string message);
}
