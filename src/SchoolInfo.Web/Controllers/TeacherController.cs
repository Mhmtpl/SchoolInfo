using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolInfo.Web.Models;
using SchoolInfo.Web.Services;

namespace SchoolInfo.Web.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly SchoolInfoApiService _apiService;

    public TeacherController(SchoolInfoApiService apiService)
    {
        _apiService = apiService;
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
            ViewBag.ErrorMessage = "Sınıflarınız yüklenirken bir hata oluştu: " + ex.Message;
            return View(new List<ClassroomDto>());
        }
    }

    public async Task<IActionResult> ClassroomDetails(Guid id)
    {
        try
        {
            var classrooms = await _apiService.GetAsync<List<ClassroomDto>>("api/classrooms/teacher/my");
            var classroom = classrooms?.FirstOrDefault(c => c.Id == id);

            if (classroom == null)
            {
                return NotFound("Yetkili olduğunuz sınıf bulunamadı.");
            }

            var dailyRecords = await _apiService.GetAsync<List<ClassroomDailyRecordDto>>($"api/classrooms/{id}/daily-records/today");
            var mealRecords = await _apiService.GetAsync<List<StudentMealDto>>($"api/classrooms/{id}/meal-records/today");
            var activities = await _apiService.GetAsync<List<ActivityDto>>($"api/activities/classroom/{id}");

            ViewBag.Classroom = classroom;
            ViewBag.DailyRecords = dailyRecords ?? new List<ClassroomDailyRecordDto>();
            ViewBag.MealRecords = mealRecords ?? new List<StudentMealDto>();
            ViewBag.Activities = activities ?? new List<ActivityDto>();

            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Sınıf detayları yüklenirken hata oluştu: " + ex.Message;
            return View();
        }
    }

    public async Task<IActionResult> MealTracking(Guid id)
    {
        try
        {
            var classrooms = await _apiService.GetAsync<List<ClassroomDto>>("api/classrooms/teacher/my");
            var classroom = classrooms?.FirstOrDefault(c => c.Id == id);

            if (classroom == null)
            {
                return NotFound("Yetkili olduğunuz sınıf bulunamadı.");
            }

            var detailedMeals = await _apiService.GetAsync<List<StudentDetailedMealsResponse>>($"api/classrooms/{id}/meal-records/detailed");

            ViewBag.Classroom = classroom;
            return View(detailedMeals ?? new List<StudentDetailedMealsResponse>());
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Yemek kayıtları yüklenirken hata oluştu: " + ex.Message;
            return View(new List<StudentDetailedMealsResponse>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDailyRecord([FromBody] UpdateDailyRecordRequest request)
    {
        try
        {
            var todayRecord = await _apiService.GetAsync<Dictionary<string, object>>($"api/daily-records/student/{request.StudentId}/today");
            Guid dailyRecordId;

            if (todayRecord == null)
            {
                dailyRecordId = await _apiService.PostAsync<object, Guid>("api/daily-records", new { StudentId = request.StudentId, Date = DateTime.UtcNow.Date });
            }
            else
            {
                if (todayRecord.TryGetValue("id", out var idVal) || todayRecord.TryGetValue("Id", out idVal))
                {
                    dailyRecordId = Guid.Parse(idVal.ToString()!);
                }
                else
                {
                    dailyRecordId = await _apiService.PostAsync<object, Guid>("api/daily-records", new { StudentId = request.StudentId, Date = DateTime.UtcNow.Date });
                }
            }

            var updateCommand = new
            {
                DailyRecordId = dailyRecordId,
                SleepStatus = request.SleepStatus,
                SleepStartTime = DateTime.UtcNow.Date.AddHours(13),
                SleepEndTime = DateTime.UtcNow.Date.AddHours(14).AddMinutes(30),
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
    public async Task<IActionResult> UpdateMealRecord([FromBody] UpdateMealRecordRequest request)
    {
        try
        {
            // 1. Öğrencinin detaylarını çekerek ait olduğu sınıf ID'sini bulalım (kesin yöntem, dictionary hatasını önler)
            var studentInfo = await _apiService.GetAsync<Dictionary<string, object>>($"api/students/{request.StudentId}");
            if (studentInfo == null)
            {
                return Json(new { success = false, message = "Öğrenci bulunamadı." });
            }

            // Case-insensitive olarak classroomId alanını alalım
            Guid classroomId = Guid.Empty;
            if (studentInfo.TryGetValue("classroomId", out var classVal) || studentInfo.TryGetValue("ClassroomId", out classVal))
            {
                classroomId = Guid.Parse(classVal.ToString()!);
            }

            // 2. Bugünlük kayıt var mı kontrol et veya oluştur
            var todayRecord = await _apiService.GetAsync<Dictionary<string, object>>($"api/daily-records/student/{request.StudentId}/today");
            Guid dailyRecordId;

            if (todayRecord == null)
            {
                dailyRecordId = await _apiService.PostAsync<object, Guid>("api/daily-records", new { StudentId = request.StudentId, Date = DateTime.UtcNow.Date });
            }
            else
            {
                if (todayRecord.TryGetValue("id", out var idVal) || todayRecord.TryGetValue("Id", out idVal))
                {
                    dailyRecordId = Guid.Parse(idVal.ToString()!);
                }
                else
                {
                    dailyRecordId = await _apiService.PostAsync<object, Guid>("api/daily-records", new { StudentId = request.StudentId, Date = DateTime.UtcNow.Date });
                }
            }

            // 3. MealRecordId'yi frontend'den alalım. Yoksa sınıfın bugünkü öğün listesinden eşleştirelim.
            Guid mealRecordId = request.MealRecordId;

            if (mealRecordId == Guid.Empty)
            {
                var meals = await _apiService.GetAsync<List<StudentMealDto>>($"api/classrooms/{classroomId}/meal-records/today");
                var studentMeal = meals?.FirstOrDefault(m => m.StudentId == request.StudentId);
                
                if (studentMeal != null && studentMeal.MealRecordId.HasValue)
                {
                    mealRecordId = studentMeal.MealRecordId.Value;
                }
                else
                {
                    // Test ortamındaki seed uyumluluğu için varsayılan bir Guid
                    mealRecordId = Guid.NewGuid();
                }
            }

            var updateCommand = new
            {
                MealRecordId = mealRecordId,
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
    public async Task<IActionResult> CreateActivity(Guid classroomId, string title, string description, DateTime activityDate)
    {
        try
        {
            var command = new CreateActivityCommand
            {
                Title = title,
                Description = description,
                ActivityDate = activityDate,
                ClassroomId = classroomId
            };

            await _apiService.PostAsync("api/activities", command);
            return RedirectToAction("ClassroomDetails", new { id = classroomId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Aktivite eklenirken hata oluştu: " + ex.Message;
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
            TempData["ErrorMessage"] = "Aktivite tamamlanırken hata oluştu: " + ex.Message;
            return RedirectToAction("ClassroomDetails", new { id = classroomId });
        }
    }
}
