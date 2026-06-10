using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolInfo.Application.Features.Classrooms.Commands.ApplyWeeklySchedule;

public record ApplyWeeklyScheduleCommand(Guid ClassroomId) : IRequest<bool>;

public class ApplyWeeklyScheduleCommandHandler : IRequestHandler<ApplyWeeklyScheduleCommand, bool>
{
    private readonly IAppDbContext _dbContext;

    public ApplyWeeklyScheduleCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(ApplyWeeklyScheduleCommand request, CancellationToken cancellationToken)
    {
        var classroom = await _dbContext.Classrooms.FindAsync(new object[] { request.ClassroomId }, cancellationToken);
        if (classroom == null) return false;

        var templateSchedules = await _dbContext.ClassroomWeeklySchedules
            .Where(x => x.ClassroomId == request.ClassroomId)
            .ToListAsync(cancellationToken);

        if (!templateSchedules.Any()) return false; // Şablon yoksa işlem yapma

        // Geçerli haftanın Pazartesi gününü bul
        var today = DateTime.UtcNow.AddHours(3).Date;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = today.AddDays(-1 * diff).Date;

        // O haftadaki mevcut aktiviteleri çek (aynı saatlere çakışma kontrolü veya direkt silip yazma)
        // Kullanıcı dostu olması adına şablon uygulandığında o haftanın tüm programını temizleyip yeniden basabiliriz.
        // Ama manuel girilenleri silmemek için, sadece şablondan gelen saatlerle çakışanları veya "Tüm haftayı" temizlemek opsiyoneldir.
        // Biz kolaylık için bu haftaki tüm aktiviteleri silip şablonu basacağız (Öğretmen sıfırdan şablon uyguluyor).
        var startOfWeek = monday;
        var endOfWeek = monday.AddDays(7);

        var existingActivities = await _dbContext.Activities
            .Where(a => a.ClassroomId == request.ClassroomId && a.ActivityDate >= startOfWeek && a.ActivityDate < endOfWeek)
            .ToListAsync(cancellationToken);

        _dbContext.Activities.RemoveRange(existingActivities);

        // Yeni aktiviteleri şablondan üret
        foreach (var ts in templateSchedules)
        {
            // ts.DayOfWeek: 1 (Pazartesi) ile 5 (Cuma) arası
            int daysToAdd = ts.DayOfWeek - 1;
            var targetDate = monday.AddDays(daysToAdd);

            var activity = new Activity(
                title: ts.Title,
                description: ts.Description ?? "",
                activityDate: targetDate,
                startTime: ts.StartTime,
                endTime: ts.EndTime,
                type: ts.Type,
                classroomId: request.ClassroomId
            )
            {
                SchoolId = classroom.SchoolId
            };

            await _dbContext.Activities.AddAsync(activity, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
