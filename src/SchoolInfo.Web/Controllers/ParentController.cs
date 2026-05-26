using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolInfo.Web.Models;
using SchoolInfo.Web.Services;

namespace SchoolInfo.Web.Controllers;

[Authorize(Roles = "Parent")]
public class ParentController : Controller
{
    private readonly SchoolInfoApiService _apiService;

    public ParentController(SchoolInfoApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var userEmail = User.Identity?.Name;
            var children = new List<ClassroomStudentDto>();

            if (!string.IsNullOrEmpty(userEmail))
            {
                var myStudents = await _apiService.GetAsync<List<Dictionary<string, object>>>("api/students/my-children");
                
                if (myStudents != null)
                {
                    foreach (var studentInfo in myStudents)
                    {
                        children.Add(new ClassroomStudentDto
                        {
                            Id = Guid.Parse(studentInfo["id"].ToString()!),
                            FirstName = studentInfo["firstName"].ToString()!,
                            LastName = studentInfo["lastName"].ToString()!
                        });
                    }
                }
            }

            return View(children);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Çocuklarınızın listesi yüklenirken bir hata oluştu: " + ex.Message;
            return View(new List<ClassroomStudentDto>());
        }
    }

    public async Task<IActionResult> ChildDetails(Guid id)
    {
        try
        {
            // 1. Çocuğun temel bilgilerini çekelim (ad, soyad, sınıf vb.)
            var studentInfo = await _apiService.GetAsync<Dictionary<string, object>>($"api/students/{id}");
            if (studentInfo == null)
            {
                return NotFound("Öğrenci bulunamadı.");
            }

            var classroomId = Guid.Parse(studentInfo["classroomId"].ToString()!);
            
            // Sınıf adını GetClassroom endpoint'inden dinamik çekerek sözlük hatasını gideriyoruz
            string classroomName = "Sınıf Bilgisi Yok";
            try
            {
                var classroomInfo = await _apiService.GetAsync<Dictionary<string, object>>($"api/classrooms/{classroomId}");
                if (classroomInfo != null && (classroomInfo.TryGetValue("name", out var nameVal) || classroomInfo.TryGetValue("Name", out nameVal)))
                {
                    classroomName = nameVal.ToString()!;
                }
            }
            catch
            {
                // Hata durumunda varsayılan değer korunsun
            }

            // 2. Çocuğun bugünkü günlük özbakım kaydını çekelim (orijinal API endpoint'i)
            ClassroomDailyRecordDto? myDailyRecord = null;
            try
            {
                var todayRecord = await _apiService.GetAsync<Dictionary<string, object>>($"api/daily-records/student/{id}/today");
                if (todayRecord != null)
                {
                    myDailyRecord = new ClassroomDailyRecordDto
                    {
                        StudentId = id,
                        FirstName = studentInfo["firstName"].ToString()!,
                        LastName = studentInfo["lastName"].ToString()!,
                        HasRecordToday = true,
                        WaterIntake = todayRecord.TryGetValue("waterIntake", out var water) ? Convert.ToInt32(water.ToString()) : 200,
                        SleepStatus = todayRecord.TryGetValue("sleepStatus", out var sleep) ? 
                            (sleep.ToString()!.Equals("SleptWell", StringComparison.OrdinalIgnoreCase) ? 2 : 
                             sleep.ToString()!.Equals("SleptLittle", StringComparison.OrdinalIgnoreCase) ? 1 : 0) : 2,
                        TeacherNotes = todayRecord.TryGetValue("teacherNote", out var note) ? note?.ToString() : ""
                    };
                }
            }
            catch
            {
                // Bugünlük kayıt henüz girilmemiş olabilir, bunu sessizce ele alıyoruz
            }

            // 3. Sınıf aktivitelerini çekelim (orijinal API endpoint'i)
            var activities = await _apiService.GetAsync<List<ActivityDto>>($"api/activities/classroom/{classroomId}");

            // 4. Çocuğun AI günlük raporunu (özetini) çekelim (orijinal API endpoint'i)
            var aiSummaryText = "Bugün için henüz bir yapay zeka özeti oluşturulmadı.";
            try
            {
                var aiSummaryRaw = await _apiService.GetAsync<Dictionary<string, string>>($"api/summary/student/{id}/today");
                if (aiSummaryRaw != null && aiSummaryRaw.TryGetValue("content", out var summary))
                {
                    aiSummaryText = summary;
                }
            }
            catch
            {
                // Özet bulunamazsa varsayılan mesaj kalır
            }

            // 3. Çocuğun detaylı yemek kayıtlarını (kahvaltı, öğle, ikindi) çekelim
            StudentDetailedMealsResponse? detailedMeals = null;
            try
            {
                var allDetailedMeals = await _apiService.GetAsync<List<StudentDetailedMealsResponse>>($"api/classrooms/{classroomId}/meal-records/detailed");
                detailedMeals = allDetailedMeals?.FirstOrDefault(m => m.StudentId == id);
            }
            catch
            {
                // Sessizce yut
            }

            // 5. Sınıf Bültenlerini çekelim
            var newsletters = new List<NewsletterDto>();
            try
            {
                newsletters = await _apiService.GetAsync<List<NewsletterDto>>($"api/newsletters/classroom/{classroomId}") ?? new List<NewsletterDto>();
            }
            catch
            {
                // Sessizce yut
            }

            ViewBag.StudentName = $"{studentInfo["firstName"]} {studentInfo["lastName"]}";
            ViewBag.ClassroomName = classroomName;
            ViewBag.DailyRecord = myDailyRecord;
            ViewBag.DetailedMeals = detailedMeals;
            ViewBag.Activities = activities ?? new List<ActivityDto>();
            ViewBag.AiSummary = aiSummaryText;
            ViewBag.Newsletters = newsletters;

            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Çocuk detayları yüklenirken hata oluştu: " + ex.Message;
            return View();
        }
    }
}
