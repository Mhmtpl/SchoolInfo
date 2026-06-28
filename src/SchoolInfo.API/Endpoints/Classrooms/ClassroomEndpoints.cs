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
using SchoolInfo.Domain.Entities;

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

        group.MapGet("/{id:guid}/daily-records/today", async (System.Guid id, DateTime? date, IMediator mediator) =>
        {
            var targetDate = date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomDailyRecords.GetClassroomDailyRecordsQuery(id, targetDate));
            return Results.Ok(result);
        })
        .WithName("GetClassroomDailyRecordsToday")
        .WithSummary("Sınıftaki tüm öğrencilerin bugünkü günlük kayıtlarını listeler.")
        .WithDescription("Öğretmenin veya yöneticinin tek ekranda sınıfın tamamını görebilmesi için bugünün özbakım, uyku ve su tüketim kayıtlarını döner.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString(), SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapGet("/{id:guid}/meal-records/today", async (System.Guid id, DateTime? date, IMediator mediator) =>
        {
            var targetDate = date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomMealRecords.GetClassroomMealRecordsQuery(id, targetDate));
            return Results.Ok(result);
        })
        .WithName("GetClassroomMealRecordsToday")
        .WithSummary("Sınıftaki tüm öğrencilerin bugünkü yemek kayıtlarını listeler.")
        .WithDescription("Öğretmenin veya yöneticinin tek ekranda sınıfın tamamının yemek durumunu görebilmesi için bugünün öğün kayıtlarını döner.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString(), SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));

        group.MapGet("/{id:guid}/meal-records/detailed", async (System.Guid id, DateTime? date, SchoolInfo.Application.Common.Interfaces.IAppDbContext dbContext, SchoolInfo.Application.Common.Interfaces.ICurrentUserService currentUserService) =>
        {
            var today = date.HasValue ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.UtcNow.AddHours(3).Date, DateTimeKind.Utc);
            var dayOfWeek = today.DayOfWeek;

            var students = await dbContext.Students
                .Where(s => s.ClassroomId == id && !s.IsDeleted)
                .ToListAsync();

            var studentIds = students.Select(s => s.Id).ToList();

            var dailyRecords = await dbContext.DailyRecords
                .Where(r => studentIds.Contains(r.StudentId) && r.Date == today && !r.IsDeleted)
                .ToListAsync();

            var dailyRecordIds = dailyRecords.Select(d => d.Id).ToList();

            var mealRecords = await dbContext.MealRecords
                .Where(m => dailyRecordIds.Contains(m.DailyRecordId) && !m.IsDeleted)
                .ToListAsync();

            // Sınıfın haftalık şablonunu çekelim (bugün için olanları)
            var weeklyPlans = await dbContext.WeeklyMealPlans
                .Where(w => w.ClassroomId == id && w.DayOfWeek == dayOfWeek && !w.IsDeleted)
                .ToListAsync();

            var breakfastPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("Kahvaltı", StringComparison.OrdinalIgnoreCase));
            var lunchPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("Öğle Yemeği", StringComparison.OrdinalIgnoreCase));
            var snackPlan = weeklyPlans.FirstOrDefault(w => w.MealName.Equals("İkindi Kahvaltısı", StringComparison.OrdinalIgnoreCase));

            var result = students.Select(student =>
            {
                var dailyRecord = dailyRecords.FirstOrDefault(d => d.StudentId == student.Id);
                var studentMeals = new List<object>();

                var mealNames = new[] { "Kahvaltı", "Öğle Yemeği", "İkindi Kahvaltısı" };
                foreach (var mealName in mealNames)
                {
                    MealRecord? m = null;
                    if (dailyRecord != null)
                    {
                        m = mealRecords.FirstOrDefault(x => x.DailyRecordId == dailyRecord.Id && x.MealName.Equals(mealName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (m != null)
                    {
                        studentMeals.Add(new
                        {
                            MealRecordId = m.Id,
                            MealName = m.MealName,
                            StatusType = (int)m.Status.Type,
                            StatusDescription = m.Status.Description,
                            PlannedCalories = m.PlannedCalories,
                            FoodContent = m.FoodContent,
                            ProteinGrams = m.ProteinGrams,
                            CarbsGrams = m.CarbsGrams
                        });
                    }
                    else
                    {
                        var plan = mealName.Equals("Kahvaltı", StringComparison.OrdinalIgnoreCase) ? breakfastPlan :
                                   mealName.Equals("Öğle Yemeği", StringComparison.OrdinalIgnoreCase) ? lunchPlan :
                                   snackPlan;

                        studentMeals.Add(new
                        {
                            MealRecordId = Guid.Empty, // Kayıt yok, öğretmen ilk kaydettiğinde oluşacak
                            MealName = mealName,
                            StatusType = 0, // None / Hiç Yemedi
                            StatusDescription = "",
                            PlannedCalories = plan?.PlannedCalories,
                            FoodContent = plan?.FoodContent,
                            ProteinGrams = plan?.ProteinGrams,
                            CarbsGrams = plan?.CarbsGrams
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
        .WithSummary("Sınıftaki öğrencilerin bugünkü tüm detaylı yemek kayıtlarını listeler (Gerekirse şablondan otomatik oluşturur).")
        .RequireAuthorization();

        group.MapPut("/{id:guid}/meal-records/plan", async (System.Guid id, UpdateClassroomMealPlanRequest request, SchoolInfo.Application.Common.Interfaces.IAppDbContext dbContext) =>
        {
            var today = System.DateTime.UtcNow.AddHours(3).Date;
            var students = await dbContext.Students
                .Where(s => s.ClassroomId == id && !s.IsDeleted)
                .ToListAsync();

            if (!students.Any())
            {
                return Results.Ok();
            }

            var studentIds = students.Select(s => s.Id).ToList();

            var dailyRecords = await dbContext.DailyRecords
                .Where(r => studentIds.Contains(r.StudentId) && r.Date == today && !r.IsDeleted)
                .ToListAsync();

            var dailyRecordIds = dailyRecords.Select(d => d.Id).ToList();

            var mealRecords = await dbContext.MealRecords
                .Where(m => dailyRecordIds.Contains(m.DailyRecordId) && !m.IsDeleted)
                .ToListAsync();

            foreach (var student in students)
            {
                var dailyRecord = dailyRecords.FirstOrDefault(d => d.StudentId == student.Id);
                if (dailyRecord == null)
                {
                    dailyRecord = new SchoolInfo.Domain.Entities.DailyRecord(student.Id, today);
                    dailyRecord.SchoolId = student.SchoolId;
                    await dbContext.DailyRecords.AddAsync(dailyRecord);
                    dailyRecords.Add(dailyRecord);
                }

                foreach (var mealDto in request.Meals)
                {
                    var mealRecord = mealRecords.FirstOrDefault(m => m.DailyRecordId == dailyRecord.Id && m.MealName.Equals(mealDto.MealName, System.StringComparison.OrdinalIgnoreCase));
                    if (mealRecord == null)
                    {
                        mealRecord = new SchoolInfo.Domain.Entities.MealRecord(
                            dailyRecord.Id, 
                            mealDto.MealName, 
                            new SchoolInfo.Domain.ValueObjects.MealStatus(SchoolInfo.Domain.Enums.MealStatusType.None, "")
                        );
                        mealRecord.SchoolId = student.SchoolId;
                        mealRecord.SetNutrition(mealDto.PlannedCalories, mealDto.FoodContent, mealDto.ProteinGrams, mealDto.CarbsGrams);
                        await dbContext.MealRecords.AddAsync(mealRecord);
                        mealRecords.Add(mealRecord);
                    }
                    else
                    {
                        mealRecord.SetNutrition(mealDto.PlannedCalories, mealDto.FoodContent, mealDto.ProteinGrams, mealDto.CarbsGrams);
                    }
                }
            }

            await dbContext.SaveChangesAsync(System.Threading.CancellationToken.None);
            return Results.Ok();
        })
        .WithName("UpdateClassroomMealPlan")
        .WithSummary("Sınıftaki tüm öğrencilerin bugünkü yemek planlarını günceller.")
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

        group.MapGet("/{id:guid}/weekly-meal-plans", async (System.Guid id, SchoolInfo.Application.Common.Interfaces.IAppDbContext dbContext) =>
        {
            var plans = await dbContext.WeeklyMealPlans
                .Where(w => w.ClassroomId == id && !w.IsDeleted)
                .OrderBy(w => w.DayOfWeek)
                .Select(w => new WeeklyMealPlanDto
                {
                    Id = w.Id,
                    DayOfWeek = (int)w.DayOfWeek,
                    MealName = w.MealName,
                    PlannedCalories = w.PlannedCalories,
                    FoodContent = w.FoodContent,
                    ProteinGrams = w.ProteinGrams,
                    CarbsGrams = w.CarbsGrams
                })
                .ToListAsync();

            return Results.Ok(plans);
        })
        .WithName("GetClassroomWeeklyMealPlans")
        .WithSummary("Sınıfın haftalık yemek şablonlarını listeler.")
        .RequireAuthorization();

        group.MapPut("/{id:guid}/weekly-meal-plans", async (System.Guid id, UpdateClassroomWeeklyMealPlanRequest request, SchoolInfo.Application.Common.Interfaces.IAppDbContext dbContext, SchoolInfo.Application.Common.Interfaces.ICurrentUserService currentUserService) =>
        {
            var classroom = await dbContext.Classrooms.FindAsync(id);
            if (classroom == null)
            {
                return Results.NotFound("Sınıf bulunamadı.");
            }

            var schoolId = classroom.SchoolId;

            var existingPlans = await dbContext.WeeklyMealPlans
                .Where(w => w.ClassroomId == id && !w.IsDeleted)
                .ToListAsync();

            foreach (var item in request.Plans)
            {
                var day = (DayOfWeek)item.DayOfWeek;
                var plan = existingPlans.FirstOrDefault(w => w.DayOfWeek == day && w.MealName.Equals(item.MealName, StringComparison.OrdinalIgnoreCase));
                
                if (plan != null)
                {
                    plan.UpdatePlan(item.PlannedCalories, item.FoodContent, item.ProteinGrams, item.CarbsGrams);
                }
                else
                {
                    var newPlan = new WeeklyMealPlan(id, day, item.MealName, item.PlannedCalories, item.FoodContent, item.ProteinGrams, item.CarbsGrams, schoolId);
                    await dbContext.WeeklyMealPlans.AddAsync(newPlan);
                }
            }

            await dbContext.SaveChangesAsync();
            return Results.Ok();
        })
        .WithName("UpdateClassroomWeeklyMealPlans")
        .WithSummary("Sınıfın haftalık yemek şablonlarını günceller veya yeni ekler.")
        .RequireAuthorization();

        group.MapGet("/{id:guid}/weekly-schedule", async (System.Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Queries.GetClassroomWeeklySchedule.GetClassroomWeeklyScheduleQuery(id));
            return Results.Ok(result);
        })
        .WithName("GetClassroomWeeklySchedule")
        .WithSummary("Sınıfın sabit haftalık ders programı şablonunu döner.")
        .RequireAuthorization();

        group.MapPut("/{id:guid}/weekly-schedule", async (System.Guid id, SchoolInfo.Application.Features.Classrooms.Commands.UpdateClassroomWeeklySchedule.UpdateClassroomWeeklyScheduleCommand command, IMediator mediator) =>
        {
            var cmd = new SchoolInfo.Application.Features.Classrooms.Commands.UpdateClassroomWeeklySchedule.UpdateClassroomWeeklyScheduleCommand(id, command.Schedules);
            await mediator.Send(cmd);
            return Results.NoContent();
        })
        .WithName("UpdateClassroomWeeklySchedule")
        .WithSummary("Sınıfın sabit haftalık ders programı şablonunu günceller.")
        .RequireAuthorization();

        group.MapPost("/{id:guid}/weekly-schedule/apply", async (System.Guid id, IMediator mediator) =>
        {
            var success = await mediator.Send(new SchoolInfo.Application.Features.Classrooms.Commands.ApplyWeeklySchedule.ApplyWeeklyScheduleCommand(id));
            return success ? Results.Ok() : Results.BadRequest("Şablon uygulanamadı veya şablon boş.");
        })
        .WithName("ApplyWeeklyScheduleToWeek")
        .WithSummary("Sınıfın şablonunu geçerli haftaya aktarır.")
        .RequireAuthorization();

        group.MapPost("/{id:guid}/ai-update", async (System.Guid id, AIClassroomUpdateRequest request, IMediator mediator) =>
        {
            var command = new SchoolInfo.Application.Features.Classrooms.Commands.AIClassroomUpdate.AIClassroomUpdateCommand(id, request.Command, request.DateStr);
            var result = await mediator.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("AIClassroomUpdate")
        .WithSummary("Yapay zeka ile sınıf günlük gelişim ve yemek kayıtlarını toplu günceller.")
        .RequireAuthorization(policy => policy.RequireRole(SchoolInfo.Domain.Enums.UserRole.Teacher.ToString(), SchoolInfo.Domain.Enums.UserRole.Admin.ToString()));
    }
}

public class AIClassroomUpdateRequest
{
    public string Command { get; set; } = string.Empty;
    public string DateStr { get; set; } = string.Empty;
}

public class ClassroomMealPlanDto
{
    public string MealName { get; set; } = string.Empty;
    public int PlannedCalories { get; set; }
    public string FoodContent { get; set; } = string.Empty;
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
}

public class UpdateClassroomMealPlanRequest
{
    public List<ClassroomMealPlanDto> Meals { get; set; } = new();
}

public class WeeklyMealPlanDto
{
    public Guid Id { get; set; }
    public int DayOfWeek { get; set; }
    public string MealName { get; set; } = string.Empty;
    public int PlannedCalories { get; set; }
    public string FoodContent { get; set; } = string.Empty;
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
}

public class UpdateClassroomWeeklyMealPlanRequest
{
    public List<WeeklyMealPlanDto> Plans { get; set; } = new();
}
