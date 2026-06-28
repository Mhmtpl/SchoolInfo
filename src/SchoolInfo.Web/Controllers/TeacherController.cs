using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SchoolInfo.Web.Models;
using SchoolInfo.Web.Services;

namespace SchoolInfo.Web.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly SchoolInfoApiService _apiService;
    private readonly ILogger<TeacherController> _logger;

    public TeacherController(SchoolInfoApiService apiService, ILogger<TeacherController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var classrooms = await _apiService.GetAsync<List<ClassroomDto>>("api/classrooms/teacher/my");
            return View(classrooms ?? new List<ClassroomDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Öğretmen sınıfları yüklenemedi.");
            ViewBag.ErrorMessage = "Sınıflarınız yüklenirken bir hata oluştu. Lütfen tekrar deneyin.";
            return View(new List<ClassroomDto>());
        }
    }

    public async Task<IActionResult> ClassroomDetails(Guid id, string? date = null)
    {
        try
        {
            var today = DateTime.UtcNow.AddHours(3).Date;
            DateTime targetDate = today;

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
            {
                targetDate = parsedDate.Date;
                if (targetDate < today.AddDays(-2) || targetDate > today)
                {
                    return RedirectToAction("ClassroomDetails", new { id = id, date = today.ToString("yyyy-MM-dd") });
                }
            }

            var dateStr = targetDate.ToString("yyyy-MM-dd");

            var classrooms = await _apiService.GetAsync<List<ClassroomDto>>("api/classrooms/teacher/my");
            var classroom = classrooms?.FirstOrDefault(c => c.Id == id);

            if (classroom == null)
            {
                return NotFound("Yetkili olduğunuz sınıf bulunamadı.");
            }

            var dailyRecords = await _apiService.GetAsync<List<ClassroomDailyRecordDto>>($"api/classrooms/{id}/daily-records/today?date={dateStr}");
            var mealRecords = await _apiService.GetAsync<List<StudentMealDto>>($"api/classrooms/{id}/meal-records/today?date={dateStr}");
            var activities = await _apiService.GetAsync<List<ActivityDto>>($"api/activities/classroom/{id}");
            var newsletters = await _apiService.GetAsync<List<NewsletterDto>>($"api/newsletters/classroom/{id}");
            var medicationRecords = await _apiService.GetAsync<List<MedicationRecordDto>>($"api/medication-records/classroom/{id}/today?date={dateStr}");
            var detailedMeals = await _apiService.GetAsync<List<StudentDetailedMealsResponse>>($"api/classrooms/{id}/meal-records/detailed?date={dateStr}");

            ViewBag.Classroom = classroom;
            ViewBag.DailyRecords = dailyRecords ?? new List<ClassroomDailyRecordDto>();
            ViewBag.MealRecords = mealRecords ?? new List<StudentMealDto>();
            ViewBag.Activities = activities ?? new List<ActivityDto>();
            ViewBag.Newsletters = newsletters ?? new List<NewsletterDto>();
            ViewBag.MedicationRecords = medicationRecords ?? new List<MedicationRecordDto>();
            ViewBag.DetailedMeals = detailedMeals ?? new List<StudentDetailedMealsResponse>();
            ViewBag.SelectedDate = dateStr;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sınıf detayları yüklenemedi. ClassroomId={Id}", id);
            ViewBag.ErrorMessage = "Sınıf detayları yüklenirken hata oluştu. Lütfen sayfayı yenileyin.";
            return View();
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDailyRecord([FromBody] UpdateDailyRecordRequest request)
    {
        try
        {
            var dateStr = request.Date ?? DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-dd");
            var todayRecord = await _apiService.GetAsync<Dictionary<string, object>>($"api/daily-records/student/{request.StudentId}/today?date={dateStr}");
            Guid dailyRecordId;

            var targetDate = DateTime.SpecifyKind(DateTime.Parse(dateStr).Date, DateTimeKind.Utc);

            if (todayRecord == null)
            {
                dailyRecordId = await _apiService.PostAsync<object, Guid>("api/daily-records", new { StudentId = request.StudentId, Date = targetDate });
            }
            else
            {
                if (todayRecord.TryGetValue("id", out var idVal) || todayRecord.TryGetValue("Id", out idVal))
                {
                    dailyRecordId = Guid.Parse(idVal.ToString()!);
                }
                else
                {
                    dailyRecordId = await _apiService.PostAsync<object, Guid>("api/daily-records", new { StudentId = request.StudentId, Date = targetDate });
                }
            }

            var updateCommand = new
            {
                DailyRecordId = dailyRecordId,
                SleepStatus = request.SleepStatus,
                SleepStartTime = targetDate.AddHours(13),
                SleepEndTime = targetDate.AddHours(14).AddMinutes(30),
                WaterAmountInMilliliters = request.WaterConsumptionInMl,
                TeacherNote = request.TeacherNote
            };

            await _apiService.PutAsync($"api/daily-records/{dailyRecordId}", updateCommand);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAttendance([FromBody] ToggleAttendanceRequest request)
    {
        try
        {
            var dateStr = request.Date;
            var targetDate = DateTime.SpecifyKind(DateTime.Parse(dateStr).Date, DateTimeKind.Utc);
            
            var todayRecord = await _apiService.GetAsync<Dictionary<string, object>>($"api/daily-records/student/{request.StudentId}/today?date={dateStr}");
            Guid dailyRecordId;

            if (todayRecord == null)
            {
                dailyRecordId = await _apiService.PostAsync<object, Guid>("api/daily-records", new { StudentId = request.StudentId, Date = targetDate });
            }
            else
            {
                if (todayRecord.TryGetValue("id", out var idVal) || todayRecord.TryGetValue("Id", out idVal))
                {
                    dailyRecordId = Guid.Parse(idVal.ToString()!);
                }
                else
                {
                    dailyRecordId = await _apiService.PostAsync<object, Guid>("api/daily-records", new { StudentId = request.StudentId, Date = targetDate });
                }
            }

            var updateCommand = new
            {
                DailyRecordId = dailyRecordId,
                SleepStatus = 0,
                SleepStartTime = targetDate.AddHours(13),
                SleepEndTime = targetDate.AddHours(14).AddMinutes(30),
                WaterAmountInMilliliters = 0,
                TeacherNote = request.IsAbsent ? "Öğrenci devamsız" : "",
                IsAbsent = request.IsAbsent
            };

            await _apiService.PutAsync($"api/daily-records/{dailyRecordId}", updateCommand);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMealRecord([FromBody] UpdateMealRecordRequest request)
    {
        try
        {
            var updateCommand = new
            {
                MealRecordId = request.MealRecordId,
                StudentId = request.StudentId,
                MealName = request.MealName,
                StatusType = request.StatusType,
                Description = request.StatusDescription
            };

            await _apiService.PutAsync($"api/meal-records/{request.StudentId}", updateCommand);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateActivity(Guid classroomId, string title, string description, DateTime activityDate, TimeSpan startTime, TimeSpan endTime, int type)
    {
        try
        {
            var command = new 
            {
                Title = title,
                Description = description,
                ActivityDate = activityDate,
                StartTime = startTime,
                EndTime = endTime,
                Type = type,
                ClassroomId = classroomId
            };

            await _apiService.PostAsync("api/activities", command);
            return RedirectToAction("ClassroomDetails", new { id = classroomId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktivite eklenemedi.");
            TempData["ErrorMessage"] = "Aktivite eklenirken hata oluştu. Lütfen tekrar deneyin.";
            return RedirectToAction("ClassroomDetails", new { id = classroomId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CompleteActivity(Guid classroomId, Guid id)
    {
        try
        {
            await _apiService.PutAsync<object>($"api/activities/{id}/complete", new { });
            return RedirectToAction("ClassroomDetails", new { id = classroomId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktivite tamamlanamadı.");
            TempData["ErrorMessage"] = "Aktivite tamamlanırken hata oluştu. Lütfen tekrar deneyin.";
            return RedirectToAction("ClassroomDetails", new { id = classroomId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateClassroomMealPlan([FromBody] UpdateClassroomMealPlanWebRequest request)
    {
        try
        {
            var apiRequest = new
            {
                Meals = request.Meals.Select(m => new
                {
                    MealName = m.MealName,
                    PlannedCalories = m.PlannedCalories,
                    FoodContent = m.FoodContent,
                    ProteinGrams = m.ProteinGrams,
                    CarbsGrams = m.CarbsGrams
                }).ToList()
            };

            await _apiService.PutAsync($"api/classrooms/{request.ClassroomId}/meal-records/plan", apiRequest);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetWeeklyMealPlans(Guid classroomId)
    {
        try
        {
            var plans = await _apiService.GetAsync<List<WeeklyMealPlanWebDto>>($"api/classrooms/{classroomId}/weekly-meal-plans");
            return Json(new { success = true, plans = plans ?? new List<WeeklyMealPlanWebDto>() });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateClassroomWeeklyMealPlan([FromBody] UpdateClassroomWeeklyMealPlanWebRequest request)
    {
        try
        {
            var apiRequest = new
            {
                Plans = request.Plans.Select(p => new
                {
                    Id = p.Id,
                    DayOfWeek = p.DayOfWeek,
                    MealName = p.MealName,
                    PlannedCalories = p.PlannedCalories,
                    FoodContent = p.FoodContent,
                    ProteinGrams = p.ProteinGrams,
                    CarbsGrams = p.CarbsGrams
                }).ToList()
            };

            await _apiService.PutAsync($"api/classrooms/{request.ClassroomId}/weekly-meal-plans", apiRequest);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("api/classrooms/{classroomId}/weekly-schedule")]
    public async Task<IActionResult> GetWeeklySchedule(Guid classroomId)
    {
        try
        {
            var schedule = await _apiService.GetAsync<List<object>>($"api/classrooms/{classroomId}/weekly-schedule");
            return Ok(schedule ?? new List<object>());
        }
        catch (Exception)
        {
            return Ok(new List<object>());
        }
    }

    [HttpPut("api/classrooms/{classroomId}/weekly-schedule")]
    public async Task<IActionResult> UpdateWeeklySchedule(Guid classroomId, [FromBody] object request)
    {
        try
        {
            await _apiService.PutAsync($"api/classrooms/{classroomId}/weekly-schedule", request);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("api/classrooms/{classroomId}/weekly-schedule/apply")]
    public async Task<IActionResult> ApplyWeeklySchedule(Guid classroomId)
    {
        try
        {
            await _apiService.PostAsync($"api/classrooms/{classroomId}/weekly-schedule/apply", new { });
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddMedicationRecord([FromBody] CreateMedicationRecordRequestWeb request)
    {
        try
        {
            var result = await _apiService.PostAsync<object, Guid>("api/medication-records", request);
            return Json(new { success = true, id = result });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateMedicationRecord([FromBody] UpdateMedicationRecordRequestWeb request)
    {
        try
        {
            await _apiService.PutAsync($"api/medication-records/{request.Id}", request);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İlaç kaydı güncellenemedi.");
            return Json(new { success = false, message = "İlaç kaydı güncellenirken hata oluştu." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMedicationRecord(Guid id)
    {
        try
        {
            await _apiService.DeleteAsync($"api/medication-records/{id}");
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerateDailySummaries(Guid classroomId, string date)
    {
        try
        {
            var students = await _apiService.GetAsync<List<ClassroomStudentDto>>($"api/classrooms/{classroomId}/students");
            if (students == null || !students.Any())
            {
                return Json(new { success = false, message = "Sınıfta öğrenci bulunamadı." });
            }

            foreach (var student in students)
            {
                await _apiService.PostAsync<object, object>($"api/summary/generate/{student.Id}?date={date}", null!);
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AIUpdateClassroom([FromBody] AIClassroomUpdateWebRequest request)
    {
        try
        {
            var dateStr = request.DateStr ?? DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-dd");
            var apiRequest = new
            {
                Command = request.Command,
                DateStr = dateStr
            };

            var response = await _apiService.PostAsync<object, AIClassroomUpdateResponseWeb>(
                $"api/classrooms/{request.ClassroomId}/ai-update", 
                apiRequest
            );

            return Json(new { success = true, result = response });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
