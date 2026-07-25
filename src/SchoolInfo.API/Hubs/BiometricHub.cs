using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.API.Hubs;

/// <summary>
/// Biyometrik verilerin gerçek zamanlı istemcilere aktarılmasını sağlayan SignalR Hub.
/// </summary>
[Authorize]
public class BiometricHub : Hub
{
    private readonly IAppDbContext _dbContext;

    public BiometricHub(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Bir öğrencinin canlı veri akışını dinlemek için gruba katılır (veli ve öğretmenler için).
    /// </summary>
    public async Task JoinStudentGroup(string studentId)
    {
        if (!Guid.TryParse(studentId, out var studentGuid))
        {
            return;
        }

        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var schoolIdClaim = Context.User?.FindFirst("SchoolId")?.Value;
        var roleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? Context.User?.FindFirst("role")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(schoolIdClaim) || string.IsNullOrEmpty(roleClaim))
        {
            return;
        }

        var userId = Guid.Parse(userIdClaim);
        var schoolId = Guid.Parse(schoolIdClaim);

        // 1. Önce öğrencinin bu okulda aktif olup olmadığını kontrol et
        var student = await ((DbContext)_dbContext).Set<Student>()
            .FirstOrDefaultAsync(s => s.Id == studentGuid && s.SchoolId == schoolId && !s.IsDeleted);

        if (student == null)
        {
            return; // Yetkisiz veya geçersiz öğrenci
        }

        // 2. Yetki Kontrolü (BOLA Önleme)
        if (roleClaim == "Parent")
        {
            var isParentOfStudent = await ((DbContext)_dbContext).Set<Student>()
                .AnyAsync(s => s.Id == studentGuid && !s.IsDeleted && s.Parents.Any(p => p.Id == userId));

            if (!isParentOfStudent)
            {
                return; // Yetkisiz veli
            }
        }
        else if (roleClaim == "Teacher")
        {
            var isAssignedToClass = await _dbContext.Classrooms
                .AnyAsync(c => c.Id == student.ClassroomId && !c.IsDeleted && c.Teachers.Any(t => t.Id == userId));

            if (!isAssignedToClass)
            {
                return; // Yetkisiz öğretmen
            }
        }
        else if (roleClaim != "Admin")
        {
            return; // Yetkisiz rol
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"Student_{studentGuid}");
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
