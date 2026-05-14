using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

namespace SchoolInfo.API.Endpoints.Summary;

/// <summary>
/// GÃ¼nlÃ¼k AI Ã¶zetleri iÃ§in Minimal API endpoint'leri.
/// </summary>
public class SummaryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/summary").WithTags("Summary").RequireAuthorization();

        group.MapPost("/generate/{studentId:guid}", GenerateSummaryAsync)
            .WithName("GenerateSummary")
            .WithSummary("Ã–ÄŸrenci iÃ§in yapay zeka destekli gÃ¼n sonu Ã¶zetini oluÅŸturur.");

        group.MapGet("/student/{studentId:guid}/today", GetSummaryTodayAsync)
            .WithName("GetSummaryToday")
            .WithSummary("Ã–ÄŸrencinin bugÃ¼nkÃ¼ AI Ã¶zetini getirir.");
    }

    private static async Task<IResult> GenerateSummaryAsync(Guid studentId, IMediator mediator)
    {
        // Not: GerÃ§ek senaryoda Ã¶nce DailyRecordId bulunur, Ã¶rnek olarak StudentId Ã¼zerinden tetikleme yapÄ±lÄ±yor.
        // Guid dailyRecordId = ...;
        // var command = new GenerateDailySummaryCommand(dailyRecordId);
        // var summaryId = await mediator.Send(command);
        return Results.Ok();
    }

    private static IResult GetSummaryTodayAsync(Guid studentId)
    {
        // Ã–rnek dÃ¶nÃ¼ÅŸ
        return Results.Ok(new { Content = "BugÃ¼n Ã§ok keyifli vakit geÃ§irdi." });
    }
}
