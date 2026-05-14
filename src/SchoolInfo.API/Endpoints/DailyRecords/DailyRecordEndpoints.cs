using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolInfo.Application.Features.DailyRecords.Commands.CreateDailyRecord;
using SchoolInfo.Application.Features.DailyRecords.Commands.UpdateDailyRecord;
using SchoolInfo.Application.Features.DailyRecords.Queries.GetStudentDailyRecord;

namespace SchoolInfo.API.Endpoints.DailyRecords;

/// <summary>
/// GÃ¼nlÃ¼k kayÄ±t (Daily Record) iÅŸlemleri iÃ§in Minimal API endpoint'leri.
/// </summary>
public class DailyRecordEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/daily-records").WithTags("Daily Records").RequireAuthorization();

        group.MapPost("/", CreateDailyRecordAsync)
            .WithName("CreateDailyRecord")
            .WithSummary("Yeni gÃ¼nlÃ¼k kayÄ±t oluÅŸturur.");

        group.MapPut("/{id:guid}", UpdateDailyRecordAsync)
            .WithName("UpdateDailyRecord")
            .WithSummary("Mevcut gÃ¼nlÃ¼k kaydÄ± gÃ¼nceller.");

        group.MapGet("/student/{studentId:guid}/today", GetStudentDailyRecordTodayAsync)
            .WithName("GetStudentDailyRecordToday")
            .WithSummary("Ã–ÄŸrencinin bugÃ¼nkÃ¼ gÃ¼nlÃ¼k kaydÄ±nÄ± getirir.");
    }

    private static async Task<IResult> CreateDailyRecordAsync(CreateDailyRecordCommand command, IMediator mediator)
    {
        var id = await mediator.Send(command);
        return Results.Created($"/api/daily-records/{id}", id);
    }

    private static async Task<IResult> UpdateDailyRecordAsync(Guid id, UpdateDailyRecordCommand command, IMediator mediator)
    {
        if (id != command.DailyRecordId)
            return Results.BadRequest("Route Ã¼zerindeki ID ile body iÃ§erisindeki ID uyuÅŸmuyor.");

        await mediator.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> GetStudentDailyRecordTodayAsync(Guid studentId, IMediator mediator)
    {
        var query = new GetStudentDailyRecordQuery(studentId, DateTime.UtcNow.Date);
        var result = await mediator.Send(query);

        return result is not null ? Results.Ok(result) : Results.NotFound();
    }
}
