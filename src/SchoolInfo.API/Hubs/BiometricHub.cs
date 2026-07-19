using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace SchoolInfo.API.Hubs;

/// <summary>
/// Biyometrik verilerin gerçek zamanlı istemcilere aktarılmasını sağlayan SignalR Hub.
/// </summary>
[Authorize]
public class BiometricHub : Hub
{
    /// <summary>
    /// Bir öğrencinin canlı veri akışını dinlemek için gruba katılır (veli ve öğretmenler için).
    /// </summary>
    public async Task JoinStudentGroup(string studentId)
    {
        if (Guid.TryParse(studentId, out var studentGuid))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Student_{studentGuid}");
        }
    }

    /// <summary>
    /// Öğrencinin grubundan ayrılır.
    /// </summary>
    public async Task LeaveStudentGroup(string studentId)
    {
        if (Guid.TryParse(studentId, out var studentGuid))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Student_{studentGuid}");
        }
    }
}
