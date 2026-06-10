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

    public async Task<IActionResult> ChildDetails(Guid id, string? date = null)
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
                    return RedirectToAction("ChildDetails", new { id = id, date = today.ToString("yyyy-MM-dd") });
                }
            }

            var dateStr = targetDate.ToString("yyyy-MM-dd");

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

            // 2. Çocuğun günlük özbakım kaydını çekelim
            ClassroomDailyRecordDto? myDailyRecord = null;
            try
            {
                var todayRecord = await _apiService.GetAsync<Dictionary<string, object>>($"api/daily-records/student/{id}/today?date={dateStr}");
                if (todayRecord != null)
                {
                    myDailyRecord = new ClassroomDailyRecordDto
                    {
                        StudentId = id,
                        FirstName = studentInfo["firstName"].ToString()!,
                        LastName = studentInfo["lastName"].ToString()!,
                        HasRecordToday = true,
                        WaterIntake = (todayRecord.TryGetValue("waterIntake", out var water) || todayRecord.TryGetValue("WaterIntake", out water)) ? Convert.ToInt32(water.ToString()) : 0,
                        SleepStatus = (todayRecord.TryGetValue("sleepStatus", out var sleep) || todayRecord.TryGetValue("SleepStatus", out sleep)) ? 
                            (sleep.ToString()!.Equals("SleptWell", StringComparison.OrdinalIgnoreCase) || sleep.ToString()!.Equals("2") ? 2 : 
                             sleep.ToString()!.Equals("SleptLittle", StringComparison.OrdinalIgnoreCase) || sleep.ToString()!.Equals("1") ? 1 : 0) : 0,
                        TeacherNotes = (todayRecord.TryGetValue("teacherNote", out var note) || todayRecord.TryGetValue("TeacherNote", out note)) ? note?.ToString() : "",
                        IsAbsent = (todayRecord.TryGetValue("isAbsent", out var absent) || todayRecord.TryGetValue("IsAbsent", out absent)) && Convert.ToBoolean(absent.ToString())
                    };
                }
            }
            catch (Exception ex)
            {
                // Günlük kayıt henüz girilmemiş veya başka bir hata olabilir. Hata durumunda loglayıp devam edelim.
                System.Diagnostics.Debug.WriteLine($"ChildDetails daily record fetch error: {ex.Message}");
            }

            // 3. Sınıf aktivitelerini çekip sadece ilgili tarihte yapılanları filtreleyelim
            var allActivities = await _apiService.GetAsync<List<ActivityDto>>($"api/activities/classroom/{classroomId}");
            var todayActivities = allActivities?.Where(a => a.ActivityDate.Date == targetDate.Date).ToList() ?? new List<ActivityDto>();

            // 4. Çocuğun AI günlük raporunu (özetini) çekelim
            var aiSummaryText = "Bugün için henüz bir yapay zeka özeti oluşturulmadı.";
            try
            {
                var aiSummaryRaw = await _apiService.GetAsync<Dictionary<string, string>>($"api/summary/student/{id}/today?date={dateStr}");
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
                var allDetailedMeals = await _apiService.GetAsync<List<StudentDetailedMealsResponse>>($"api/classrooms/{classroomId}/meal-records/detailed?date={dateStr}");
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

            // 6. Çocuğun ilaç kayıtlarını çekelim
            var medicationRecords = new List<MedicationRecordDto>();
            try
            {
                medicationRecords = await _apiService.GetAsync<List<MedicationRecordDto>>($"api/medication-records/student/{id}/today?date={dateStr}") ?? new List<MedicationRecordDto>();
            }
            catch
            {
                // Sessizce yut
            }

            // 7. Sınıfın haftalık şablon programını çekelim
            var weeklySchedule = new List<WeeklyScheduleDto>();
            try
            {
                weeklySchedule = await _apiService.GetAsync<List<WeeklyScheduleDto>>($"api/classrooms/{classroomId}/weekly-schedule") ?? new List<WeeklyScheduleDto>();
            }
            catch
            {
                // Sessizce yut
            }

            ViewBag.StudentName = $"{studentInfo["firstName"]} {studentInfo["lastName"]}";
            ViewBag.ClassroomName = classroomName;
            ViewBag.DailyRecord = myDailyRecord;
            ViewBag.DetailedMeals = detailedMeals;
            ViewBag.Activities = todayActivities;
            ViewBag.WeeklySchedule = weeklySchedule;
            ViewBag.AiSummary = aiSummaryText;
            ViewBag.Newsletters = newsletters;
            ViewBag.MedicationRecords = medicationRecords;
            ViewBag.SelectedDate = dateStr;

            return View();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Çocuk detayları yüklenirken hata oluştu: " + ex.Message;
            return View();
        }
    }
}
