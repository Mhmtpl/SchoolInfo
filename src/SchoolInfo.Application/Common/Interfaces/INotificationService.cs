using System;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// KullanÄ±cÄ±lara bildirim gÃ¶nderen servis.
/// </summary>
public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string title, string message);
}
