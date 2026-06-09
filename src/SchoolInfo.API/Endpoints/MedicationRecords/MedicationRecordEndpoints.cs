using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.API.Endpoints;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;

namespace SchoolInfo.API.Endpoints.MedicationRecords;

public class MedicationRecordEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/medication-records").WithTags("Medication Records").RequireAuthorization();

        group.MapPost("/", CreateMedicationRecordAsync)
            .WithName("CreateMedicationRecord")
            .WithSummary("Yeni ilaç kaydı oluşturur.");

        group.MapGet("/student/{studentId:guid}/today", GetStudentMedicationRecordsTodayAsync)
            .WithName("GetStudentMedicationRecordsToday")
            .WithSummary("Öğrencinin bugünkü ilaç kayıtlarını getirir.");

        group.MapGet("/classroom/{classroomId:guid}/today", GetClassroomMedicationRecordsTodayAsync)
            .WithName("GetClassroomMedicationRecordsToday")
            .WithSummary("Sınıftaki öğrencilerin bugünkü ilaç kayıtlarını getirir.");
    }

    private static async Task<IResult> CreateMedicationRecordAsync(
        MedicationRecordRequest request,
        IAppDbContext dbContext)
    {
        var today = DateTime.UtcNow.Date;
        var student = await dbContext.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId && !s.IsDeleted);
        if (student == null)
        {
            return Results.NotFound("Öğrenci bulunamadı.");
        }

        var dailyRecord = await dbContext.DailyRecords
            .FirstOrDefaultAsync(d => d.StudentId == request.StudentId && d.Date == today && !d.IsDeleted);

        if (dailyRecord == null)
        {
            dailyRecord = new DailyRecord(request.StudentId, today)
            {
                SchoolId = student.SchoolId
            };
            await dbContext.DailyRecords.AddAsync(dailyRecord);
        }

        var medicationRecord = new MedicationRecord(
            dailyRecord.Id,
            request.StudentId,
            request.MedicineName,
            request.Dosage,
            request.AdministrationTime,
            request.Taken,
            request.Note);

        medicationRecord.SchoolId = student.SchoolId;
        await dbContext.MedicationRecords.AddAsync(medicationRecord);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/api/medication-records/{medicationRecord.Id}", medicationRecord.Id);
    }

    private static async Task<IResult> GetStudentMedicationRecordsTodayAsync(
        Guid studentId,
        IAppDbContext dbContext)
    {
        var today = DateTime.UtcNow.Date;
        var records = await dbContext.MedicationRecords
            .Where(m => m.StudentId == studentId && !m.IsDeleted)
            .Where(m => dbContext.DailyRecords.Any(d => d.Id == m.DailyRecordId && d.Date == today))
            .ToListAsync();

        return Results.Ok(records.Select(m => new
        {
            m.Id,
            m.StudentId,
            m.MedicineName,
            m.Dosage,
            m.AdministrationTime,
            m.Taken,
            m.Note
        }));
    }

    private static async Task<IResult> GetClassroomMedicationRecordsTodayAsync(
        Guid classroomId,
        IAppDbContext dbContext)
    {
        var today = DateTime.UtcNow.Date;
        var studentIds = await dbContext.Students
            .Where(s => s.ClassroomId == classroomId && !s.IsDeleted)
            .Select(s => s.Id)
            .ToListAsync();

        var records = await dbContext.MedicationRecords
            .Where(m => studentIds.Contains(m.StudentId) && !m.IsDeleted)
            .Where(m => dbContext.DailyRecords.Any(d => d.Id == m.DailyRecordId && d.Date == today))
            .ToListAsync();

        return Results.Ok(records.Select(m => new
        {
            m.Id,
            m.StudentId,
            m.MedicineName,
            m.Dosage,
            m.AdministrationTime,
            m.Taken,
            m.Note
        }));
    }

    private sealed record MedicationRecordRequest(
        Guid StudentId,
        string MedicineName,
        string Dosage,
        string AdministrationTime,
        bool Taken,
        string? Note);
}
