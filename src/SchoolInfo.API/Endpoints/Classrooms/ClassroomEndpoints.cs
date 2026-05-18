using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using SchoolInfo.Application.Features.Classrooms.Commands.CreateClassroom;
using SchoolInfo.Application.Features.Classrooms.Queries.GetClassroom;
using SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomStudents;

namespace SchoolInfo.API.Endpoints.Classrooms;

public class ClassroomEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classrooms").WithTags("Classrooms").RequireAuthorization();

        group.MapPost("/", async (CreateClassroomCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Ok(new { Id = id });
        })
        .WithName("CreateClassroom")
        .WithSummary("Yeni sınıf oluşturur.")
        .WithDescription("Sadece Admin rolündeki kullanıcı sınıf oluşturabilir.");

        group.MapGet("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetClassroomQuery(id));
            return Results.Ok(result);
        })
        .WithName("GetClassroom")
        .WithSummary("Sınıf bilgilerini getirir.")
        .WithDescription("Belirtilen ID'ye sahip sınıfın adı ve okul bilgisini döner.");

        group.MapGet("/{id:guid}/students", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetClassroomStudentsQuery(id));
            return Results.Ok(result);
        })
        .WithName("GetClassroomStudents")
        .WithSummary("Sınıftaki tüm öğrencileri listeler.")
        .WithDescription("Belirtilen sınıfa kayıtlı aktif öğrencileri döner.");

        group.MapGet("/teacher/my", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetTeacherClassrooms.GetTeacherClassroomsQuery());
            return Results.Ok(result);
        })
        .WithName("GetMyClassrooms")
        .WithSummary("Öğretmenin atandığı sınıfları listeler.")
        .WithDescription("Giriş yapan öğretmenin sorumlu olduğu tüm sınıfları döner. Birden fazla sınıf olabilir.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString()));

        group.MapGet("/{id:guid}/daily-records/today", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomDailyRecords.GetClassroomDailyRecordsQuery(id));
            return Results.Ok(result);
        })
        .WithName("GetClassroomDailyRecordsToday")
        .WithSummary("Sınıftaki tüm öğrencilerin bugünkü günlük kayıtlarını listeler.")
        .WithDescription("Öğretmenin veya yöneticinin tek ekranda sınıfın tamamını görebilmesi için bugünün özbakım, uyku ve su tüketim kayıtlarını döner.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString(), SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapGet("/{id:guid}/meal-records/today", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomMealRecords.GetClassroomMealRecordsQuery(id));
            return Results.Ok(result);
        })
        .WithName("GetClassroomMealRecordsToday")
        .WithSummary("Sınıftaki tüm öğrencilerin bugünkü yemek kayıtlarını listeler.")
        .WithDescription("Öğretmenin veya yöneticinin tek ekranda sınıfın tamamının yemek durumunu görebilmesi için bugünün öğün kayıtlarını döner.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString(), SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapGet("/{id:guid}/meal-records/detailed", async (System.Guid id, SchoolInfo.Application.Common.Interfaces.IAppDbContext dbContext) =>
        {
            var today = DateTime.UtcNow.Date;
            var students = await dbContext.Students
                .Where(s => s.ClassroomId == id && !s.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var studentIds = students.Select(s => s.Id).ToList();

            var dailyRecords = await dbContext.DailyRecords
                .Where(r => studentIds.Contains(r.StudentId) && r.Date == today && !r.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var dailyRecordIds = dailyRecords.Select(d => d.Id).ToList();

            var mealRecords = await dbContext.MealRecords
                .Where(m => dailyRecordIds.Contains(m.DailyRecordId) && !m.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var result = students.Select(student =>
            {
                var dailyRecord = dailyRecords.FirstOrDefault(d => d.StudentId == student.Id);
                var studentMeals = new List<object>();

                if (dailyRecord != null)
                {
                    var meals = mealRecords.Where(m => m.DailyRecordId == dailyRecord.Id).ToList();
                    foreach (var m in meals)
                    {
                        studentMeals.Add(new
                        {
                            MealRecordId = m.Id,
                            MealName = m.MealName,
                            StatusType = (int)m.Status.Type,
                            StatusDescription = m.Status.Description
                        });
                    }
                }

                return new
                {
                    StudentId = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Meals = studentMeals
                };
            }).ToList();

            return Results.Ok(result);
        })
        .WithName("GetClassroomDetailedMeals")
        .WithSummary("Sınıftaki öğrencilerin bugünkü tüm detaylı yemek kayıtlarını listeler.")
        .RequireAuthorization();

        group.MapDelete("/{id:guid}", async (System.Guid id, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Commands.DeleteClassroom.DeleteClassroomCommand(id));
            return Results.NoContent();
        })
        .WithName("DeleteClassroom")
        .WithSummary("Sınıfı siler (soft delete).")
        .WithDescription("İçi boş olmayan (aktif öğrencisi bulunan) sınıf silinemez. Sadece Admin yapabilir.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapPost("/{classroomId:guid}/teacher/{teacherId:guid}", async (System.Guid classroomId, System.Guid teacherId, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Commands.AssignTeacher.AssignTeacherToClassroomCommand(classroomId, teacherId));
            return Results.Ok();
        })
        .WithName("AssignTeacherToClassroom")
        .WithSummary("Sınıfa öğretmen atar.")
        .WithDescription("Bir sınıfa birden fazla öğretmen atanabilir (anaokulu vb.). Sadece Admin yapabilir.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapDelete("/{classroomId:guid}/teacher/{teacherId:guid}", async (System.Guid classroomId, System.Guid teacherId, IMediator mediator) =>
        {
            await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Commands.RemoveTeacher.RemoveTeacherFromClassroomCommand(classroomId, teacherId));
            return Results.NoContent();
        })
        .WithName("RemoveTeacherFromClassroom")
        .WithSummary("Sınıftan öğretmeni çıkarır.")
        .WithDescription("Belirtilen öğretmeni sınıftan kaldırır. Sadece Admin yapabilir.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));
    }
}
