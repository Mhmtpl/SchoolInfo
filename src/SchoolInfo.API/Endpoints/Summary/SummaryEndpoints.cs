using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

namespace SchoolInfo.API.Endpoints.Summary;

/// <summary>
/// Günlük AI özetleri için Minimal API endpoint'leri.
/// </summary>
public class SummaryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/summary").WithTags("Summary").RequireAuthorization();

        group.MapPost("/generate/{studentId:guid}", GenerateSummaryAsync)
            .WithName("GenerateSummary")
            .WithSummary("Öğrenci için yapay zeka destekli gün sonu özetini oluşturur.");

        group.MapGet("/student/{studentId:guid}/today", GetSummaryTodayAsync)
            .WithName("GetSummaryToday")
            .WithSummary("Öğrencinin bugünkü AI özetini getirir.");
    }

    private static async Task<IResult> GenerateSummaryAsync(
        Guid studentId, 
        string? date, 
        IMediator mediator)
    {
        DateTime targetDate;
        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
        {
            targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
        }
        else
        {
            targetDate = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
        }

        var command = new GenerateDailySummaryCommand(studentId, targetDate);
        var summaryId = await mediator.Send(command);
        return Results.Ok(new { Id = summaryId });
    }

    private static async Task<IResult> GetSummaryTodayAsync(
        Guid studentId, 
        string? date, 
        IAppDbContext dbContext,
        IMediator mediator,
        SchoolInfo.Application.Common.Interfaces.ICurrentUserService currentUserService)
    {
        DateTime targetDate;
        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
        {
            targetDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);
        }
        else
        {
            targetDate = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
        }

        // Hafta sonu kontrolü
        if (targetDate.DayOfWeek == DayOfWeek.Saturday || targetDate.DayOfWeek == DayOfWeek.Sunday)
        {
            return Results.Ok(new { Content = "Hafta sonu okul kapalıdır. Ailenizle birlikte harika bir hafta sonu geçirmenizi dileriz! ☀️" });
        }

        // Veli yalnızca kendi çocuğunun özetine erişebilir (IDOR koruması)
        if (currentUserService.Role == "Parent")
        {
            var isMyChild = await ((Microsoft.EntityFrameworkCore.DbContext)dbContext)
                .Set<SchoolInfo.Domain.Entities.Student>()
                .Where(s => s.Id == studentId)
                .SelectMany(s => s.Parents)
                .AnyAsync(p => p.Id == currentUserService.UserId);

            if (!isMyChild)
                return Results.Forbid();
        }

        // Gün içinde gereksiz AI maliyetlerini önlemek için saat 16:00'dan önce özet üretilmesini engelleyelim
        var nowTurkey = DateTime.UtcNow.AddHours(3);
        if (targetDate.Date == nowTurkey.Date && nowTurkey.Hour < 16)
        {
            return Results.Ok(new { Content = "Bugünün yapay zeka özeti saat 16:00'dan sonra gün sonu raporuyla birlikte hazır olacaktır. 📝" });
        }

        var summary = await dbContext.DailySummaries
            .FirstOrDefaultAsync(s => s.StudentId == studentId && s.Date == targetDate);

        if (summary == null)
        {
            // Eğer o gün için henüz özet oluşturulmamışsa fakat bir günlük kayıt girilmişse velinin talebiyle anında üretelim
            var dailyRecord = await dbContext.DailyRecords
                .FirstOrDefaultAsync(r => r.StudentId == studentId && r.Date == targetDate);

            if (dailyRecord != null)
            {
                try
                {
                    var command = new GenerateDailySummaryCommand(studentId, targetDate);
                    var summaryId = await mediator.Send(command);
                    
                    var newSummary = await dbContext.DailySummaries
                        .FirstOrDefaultAsync(s => s.Id == summaryId);

                    if (newSummary != null)
                    {
                        return Results.Ok(new { Content = newSummary.Content });
                    }
                }
                catch (Exception)
                {
                    return Results.Ok(new { Content = "Bugün için yapay zeka özeti hazırlanırken bir sorun oluştu. Lütfen daha sonra tekrar deneyiniz." });
                }
            }

            return Results.Ok(new { Content = "Öğretmenimiz henüz bugünün kayıtlarını tamamlamadı. Günlük gelişim verileri girildiğinde özetiniz burada otomatik görünecektir. 📝" });
        }

        return Results.Ok(new { Content = summary.Content });
    }
}
