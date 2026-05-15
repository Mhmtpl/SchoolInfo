using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

    private static async Task<IResult> GenerateSummaryAsync(Guid studentId, IMediator mediator)
    {
        // Not: Gerçek senaryoda önce DailyRecordId bulunur, örnek olarak StudentId üzerinden tetikleme yapılıyor.
        // Guid dailyRecordId = ...;
        // var command = new GenerateDailySummaryCommand(dailyRecordId);
        // var summaryId = await mediator.Send(command);
        return Results.Ok();
    }

    private static IResult GetSummaryTodayAsync(Guid studentId)
    {
        // Örnek dönüş
        return Results.Ok(new { Content = "Bugün çok keyifli vakit geçirdi." });
    }
}
