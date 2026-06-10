using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SchoolInfo.API.Endpoints;
using SchoolInfo.Application.Features.MedicationRecords.Commands.CreateMedicationRecord;
using SchoolInfo.Application.Features.MedicationRecords.Commands.UpdateMedicationRecord;
using SchoolInfo.Application.Features.MedicationRecords.Commands.DeleteMedicationRecord;
using SchoolInfo.Application.Features.MedicationRecords.Queries.GetStudentMedicationRecordsToday;
using SchoolInfo.Application.Features.MedicationRecords.Queries.GetClassroomMedicationRecordsToday;

namespace SchoolInfo.API.Endpoints.MedicationRecords;

public class MedicationRecordEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/medication-records").WithTags("Medication Records").RequireAuthorization();

        group.MapPost("/", CreateMedicationRecordAsync)
            .WithName("CreateMedicationRecord")
            .WithSummary("Yeni ilaç kaydı oluşturur.");

        group.MapPut("/{id:guid}", UpdateMedicationRecordAsync)
            .WithName("UpdateMedicationRecord")
            .WithSummary("İlaç kaydını günceller.");

        group.MapDelete("/{id:guid}", DeleteMedicationRecordAsync)
            .WithName("DeleteMedicationRecord")
            .WithSummary("İlaç kaydını siler.");

        group.MapGet("/student/{studentId:guid}/today", GetStudentMedicationRecordsTodayAsync)
            .WithName("GetStudentMedicationRecordsToday")
            .WithSummary("Öğrencinin bugünkü ilaç kayıtlarını getirir.");

        group.MapGet("/classroom/{classroomId:guid}/today", GetClassroomMedicationRecordsTodayAsync)
            .WithName("GetClassroomMedicationRecordsToday")
            .WithSummary("Sınıftaki öğrencilerin bugünkü ilaç kayıtlarını getirir.");
    }

    private static async Task<IResult> CreateMedicationRecordAsync(
        MedicationRecordRequest request,
        IMediator mediator)
    {
        var command = new CreateMedicationRecordCommand(
            request.StudentId,
            request.MedicineName,
            request.Dosage,
            request.AdministrationTime,
            request.Taken,
            request.Note);

        var id = await mediator.Send(command);
        return Results.Created($"/api/medication-records/{id}", id);
    }

    private static async Task<IResult> UpdateMedicationRecordAsync(
        Guid id,
        UpdateMedicationRecordRequest request,
        IMediator mediator)
    {
        if (id != request.Id)
        {
            return Results.BadRequest("ID uyuşmuyor.");
        }

        var command = new UpdateMedicationRecordCommand(
            request.Id,
            request.MedicineName,
            request.Dosage,
            request.AdministrationTime,
            request.Taken,
            request.Note);

        await mediator.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteMedicationRecordAsync(
        Guid id,
        IMediator mediator)
    {
        await mediator.Send(new DeleteMedicationRecordCommand(id));
        return Results.NoContent();
    }

    private static async Task<IResult> GetStudentMedicationRecordsTodayAsync(
        Guid studentId,
        DateTime? date,
        IMediator mediator)
    {
        var targetDate = date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
        var query = new GetStudentMedicationRecordsTodayQuery(studentId, targetDate);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetClassroomMedicationRecordsTodayAsync(
        Guid classroomId,
        DateTime? date,
        IMediator mediator)
    {
        var targetDate = date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
        var query = new GetClassroomMedicationRecordsTodayQuery(classroomId, targetDate);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    private sealed record MedicationRecordRequest(
        Guid StudentId,
        string MedicineName,
        string Dosage,
        string AdministrationTime,
        bool Taken,
        string? Note);

    private sealed record UpdateMedicationRecordRequest(
        Guid Id,
        string MedicineName,
        string Dosage,
        string AdministrationTime,
        bool Taken,
        string? Note);
}
