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
/// Günlük kayıt (Daily Record) işlemleri için Minimal API endpoint'leri.
/// </summary>
public class DailyRecordEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/daily-records").WithTags("Daily Records").RequireAuthorization();

        group.MapPost("/", CreateDailyRecordAsync)
            .WithName("CreateDailyRecord")
            .WithSummary("Yeni günlük kayıt oluşturur.");

        group.MapPut("/{id:guid}", UpdateDailyRecordAsync)
            .WithName("UpdateDailyRecord")
            .WithSummary("Mevcut günlük kaydı günceller.");

        group.MapGet("/student/{studentId:guid}/today", GetStudentDailyRecordTodayAsync)
            .WithName("GetStudentDailyRecordToday")
            .WithSummary("Öğrencinin bugünkü günlük kaydını getirir.");
    }

    private static async Task<IResult> CreateDailyRecordAsync(CreateDailyRecordCommand command, IMediator mediator)
    {
        var id = await mediator.Send(command);
        return Results.Created($"/api/daily-records/{id}", id);
    }

    private static async Task<IResult> UpdateDailyRecordAsync(Guid id, UpdateDailyRecordCommand command, IMediator mediator)
    {
        if (id != command.DailyRecordId)
            return Results.BadRequest("Route üzerindeki ID ile body içerisindeki ID uyuşmuyor.");

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
