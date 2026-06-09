using System;
using System.Collections.Generic;

namespace SchoolInfo.Web.Models;

// --- AUTHENTICATION ---
public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}

// --- CLASSROOMS ---
public class ClassroomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SchoolId { get; set; }
}

public class ClassroomStudentDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

// --- DAILY RECORDS ---
public class ClassroomDailyRecordDto
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool HasRecordToday { get; set; }
    public int? SleepStatus { get; set; } // 0: DidNotSleep, 1: SleptLittle, 2: SleptWell
    public int? WaterIntake { get; set; }
    public string? TeacherNotes { get; set; }
    public bool IsAbsent { get; set; }
}

// --- MEALS ---
public class StudentMealDto
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? MealRecordId { get; set; }
    public int? Status { get; set; } // 0: None, 1: Little, 2: Half, 3: All
    public string? Notes { get; set; }
}

// --- ACTIVITIES ---
public class ActivityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Type { get; set; }
    public Guid ClassroomId { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateActivityCommand
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Type { get; set; }
    public Guid ClassroomId { get; set; }
}

// --- UPDATE REQUESTS ---
public class UpdateDailyRecordRequest
{
    public Guid StudentId { get; set; }
    public int SleepStatus { get; set; }
    public int WaterConsumptionInMl { get; set; }
    public string TeacherNote { get; set; } = string.Empty;
}

public class UpdateMealRecordRequest
{
    public Guid StudentId { get; set; }
    public Guid MealRecordId { get; set; }
    public string MealName { get; set; } = string.Empty;
    public int StatusType { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
}

public class StudentDetailedMealsResponse
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<MealRecordDetailDto> Meals { get; set; } = new();
}

public class MealRecordDetailDto
{
    public Guid MealRecordId { get; set; }
    public string MealName { get; set; } = string.Empty;
    public int StatusType { get; set; }
    public string? StatusDescription { get; set; }
    public int? PlannedCalories { get; set; }
    public string? FoodContent { get; set; }
    public double? ProteinGrams { get; set; }
    public double? CarbsGrams { get; set; }
}

// --- DAILY SUMMARIES (AI REPORT) ---
public class DailySummaryDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public DateTime Date { get; set; }
    public string SummaryText { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}

public class UpdateClassroomMealPlanWebRequest
{
    public Guid ClassroomId { get; set; }
    public List<ClassroomMealPlanWebDto> Meals { get; set; } = new();
}

public class ClassroomMealPlanWebDto
{
    public string MealName { get; set; } = string.Empty;
    public int PlannedCalories { get; set; }
    public string FoodContent { get; set; } = string.Empty;
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
}

public class WeeklyMealPlanWebDto
{
    public Guid Id { get; set; }
    public int DayOfWeek { get; set; }
    public string MealName { get; set; } = string.Empty;
    public int PlannedCalories { get; set; }
    public string FoodContent { get; set; } = string.Empty;
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
}

public class UpdateClassroomWeeklyMealPlanWebRequest
{
    public Guid ClassroomId { get; set; }
    public List<WeeklyMealPlanWebDto> Plans { get; set; } = new();
}

// --- NEWSLETTERS ---
public class NewsletterSectionDto
{
    public string Subject { get; set; } = string.Empty;
    public string ThisWeekSummary { get; set; } = string.Empty;
    public string NextWeekTopic { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
}

public class NewsletterDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? WeekName { get; set; }
    public List<NewsletterSectionDto> Sections { get; set; } = new();
    public int Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNewsletterCommand
{
    public Guid ClassroomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? WeekName { get; set; }
    public List<NewsletterSectionDto> Sections { get; set; } = new();
}

// --- ACTIVITY TEMPLATES ---
public class ActivityTemplateDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RequiredMaterials { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
}

// --- MEDICATION RECORDS ---
public class MedicationRecordDto
{
    public Guid Id { get; set; }
    public Guid DailyRecordId { get; set; }
    public Guid StudentId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string AdministrationTime { get; set; } = string.Empty;
    public bool Taken { get; set; }
    public string? Note { get; set; }
}

public class CreateMedicationRecordRequestWeb
{
    public Guid StudentId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string AdministrationTime { get; set; } = string.Empty;
    public bool Taken { get; set; }
    public string? Note { get; set; }
}

public class UpdateMedicationRecordRequestWeb
{
    public Guid Id { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string AdministrationTime { get; set; } = string.Empty;
    public bool Taken { get; set; }
    public string? Note { get; set; }
}

