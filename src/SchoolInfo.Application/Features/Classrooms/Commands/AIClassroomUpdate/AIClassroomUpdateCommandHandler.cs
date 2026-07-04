using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Enums;
using SchoolInfo.Domain.ValueObjects;

namespace SchoolInfo.Application.Features.Classrooms.Commands.AIClassroomUpdate;

/// <summary>
/// Yapay zeka ile sınıf günlük ve yemek verilerini güncelleme işlemini yürüten sınıf.
/// </summary>
public class AIClassroomUpdateCommandHandler : IRequestHandler<AIClassroomUpdateCommand, AIClassroomUpdateResultDto>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAIClassroomParser _aiParser;

    public AIClassroomUpdateCommandHandler(
        IAppDbContext dbContext,
        ICurrentUserService currentUserService,
        IAIClassroomParser aiParser)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _aiParser = aiParser;
    }

    public async Task<AIClassroomUpdateResultDto> Handle(AIClassroomUpdateCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Yapay zeka ile veri girişi yapmak için yetkiniz bulunmamaktadır.");
        }

        // Sınıfın okulla eşleştiğini doğrula ve öğretmenin bu sınıfa atanıp atanmadığını kontrol et
        if (_currentUserService.Role == "Teacher")
        {
            var isAssigned = await _dbContext.Classrooms
                .Where(c => c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted)
                .AnyAsync(c => c.Id == request.ClassroomId && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssigned)
            {
                throw new UnauthorizedAccessException("Bu sınıf üzerinde işlem yapmak için yetkiniz bulunmamaktadır.");
            }
        }
        else if (_currentUserService.Role == "Admin")
        {
            var classroomExists = await _dbContext.Classrooms
                .AnyAsync(c => c.Id == request.ClassroomId && c.SchoolId == _currentUserService.SchoolId && !c.IsDeleted, cancellationToken);

            if (!classroomExists)
            {
                throw new UnauthorizedAccessException("Sınıf bulunamadı veya bu sınıfa erişim yetkiniz yok.");
            }
        }

        // Tarih bilgisini UTC olarak ayarla
        DateTime targetDate;
        if (!DateTime.TryParse(request.DateStr, out targetDate))
        {
            targetDate = DateTime.UtcNow.AddHours(3).Date;
        }
        targetDate = DateTime.SpecifyKind(targetDate.Date, DateTimeKind.Utc);

        // Sınıftaki öğrencileri çek
        var students = await _dbContext.Students
            .Where(s => s.ClassroomId == request.ClassroomId && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!students.Any())
        {
            return new AIClassroomUpdateResultDto(false, "Bu sınıfta aktif öğrenci bulunamadı.", new List<string>());
        }


        // Öğrenci listesini JSON'a dönüştür
        var studentListDto = students.Select(s => new
        {
            id = s.Id,
            name = $"{s.FirstName} {s.LastName}"
        }).ToList();

        var studentNamesJson = JsonSerializer.Serialize(studentListDto);

        // AI ile komutu analiz et
        string aiJsonResult;
        try
        {
            aiJsonResult = await _aiParser.ParseClassroomCommandAsync(request.Command, studentNamesJson);
        }
        catch (Exception ex)
        {
            return new AIClassroomUpdateResultDto(false, $"Yapay zeka analiz hatası: {ex.Message}", new List<string>());
        }

        // JSON temizleme (Markdown kod blokları varsa arındır)
        var cleanJson = aiJsonResult.Trim();
        if (cleanJson.StartsWith("```"))
        {
            var startIndex = cleanJson.IndexOf('\n');
            var endIndex = cleanJson.LastIndexOf("```");
            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                cleanJson = cleanJson.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }

        AIUpdateResponse? parsedResponse;
        try
        {
            parsedResponse = JsonSerializer.Deserialize<AIUpdateResponse>(cleanJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });
        }
        catch (Exception ex)
        {
            return new AIClassroomUpdateResultDto(false, $"AI çıktısı ayrıştırılamadı. Yanıt: {aiJsonResult}. Hata: {ex.Message}", new List<string>());
        }

        if (parsedResponse?.Updates == null || !parsedResponse.Updates.Any())
        {
            return new AIClassroomUpdateResultDto(false, "Yapay zeka bu komuttan herhangi bir güncelleme çıkaramadı.", new List<string>());
        }

        var updatedStudentsReport = new List<string>();

        // Sınıfın haftalık yemek planını bugünün haftanın günü için çek
        var dayOfWeek = targetDate.DayOfWeek;
        var weeklyPlans = await _dbContext.WeeklyMealPlans
            .Where(w => w.ClassroomId == request.ClassroomId && w.DayOfWeek == dayOfWeek && !w.IsDeleted)
            .ToListAsync(cancellationToken);

        // Her bir öğrenci güncellemesini uygula
        foreach (var updateItem in parsedResponse.Updates)
        {
            var targetStudents = new List<Student>();
            var student = students.FirstOrDefault(s => s.Id == updateItem.StudentId);
            if (student != null)
            {
                targetStudents.Add(student);
            }
            else if (!string.IsNullOrWhiteSpace(updateItem.StudentName))
            {
                var normalizedName = updateItem.StudentName.Trim();
                if (normalizedName.Equals("herkes", StringComparison.OrdinalIgnoreCase) ||
                    normalizedName.Equals("tüm sınıf", StringComparison.OrdinalIgnoreCase) ||
                    normalizedName.Equals("tüm ogrenciler", StringComparison.OrdinalIgnoreCase) ||
                    normalizedName.Contains("tüm") ||
                    normalizedName.Contains("herkes"))
                {
                    targetStudents.AddRange(students);
                }
                else
                {
                    targetStudents.AddRange(students.Where(s =>
                        string.Equals($"{s.FirstName} {s.LastName}", normalizedName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals($"{s.LastName} {s.FirstName}", normalizedName, StringComparison.OrdinalIgnoreCase)));

                    if (!targetStudents.Any())
                    {
                        var firstNameMatches = students.Where(s =>
                            string.Equals(s.FirstName, normalizedName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s.LastName, normalizedName, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (firstNameMatches.Count == 1)
                        {
                            targetStudents.Add(firstNameMatches[0]);
                        }
                    }
                }
            }

            if (!targetStudents.Any()) continue; // Güvenlik kontrolü: Öğrenci gerçekten bu sınıfta mı?

            foreach (var targetStudent in targetStudents)
            {
                // DailyRecord kaydını bul veya oluştur
                var dailyRecord = await _dbContext.DailyRecords
                    .FirstOrDefaultAsync(r => r.StudentId == targetStudent.Id && r.Date == targetDate && !r.IsDeleted, cancellationToken);

            bool isNewRecord = false;
            if (dailyRecord == null)
            {
                dailyRecord = new DailyRecord(targetStudent.Id, targetDate)
                {
                    SchoolId = targetStudent.SchoolId
                };
                await _dbContext.DailyRecords.AddAsync(dailyRecord, cancellationToken);
                isNewRecord = true;
            }

            var reportParts = new List<string>();
            var dailyUpdateApplied = false;

            // 1. Günlük Kayıt (Daily Record) güncellemeleri
            if (updateItem.UpdateDailyRecord || updateItem.SleepStatus.HasValue || updateItem.WaterIntake.HasValue || updateItem.TeacherNote != null || updateItem.IsAbsent.HasValue)
            {
                if (updateItem.SleepStatus.HasValue)
                {
                    var sleepStatus = (SleepStatus)updateItem.SleepStatus.Value;
                    dailyRecord.UpdateSleepInfo(new SleepData(sleepStatus, targetDate.AddHours(13), targetDate.AddHours(14).AddMinutes(30)));
                    reportParts.Add($"Uyku: {GetSleepStatusText(sleepStatus)}");
                    dailyUpdateApplied = true;
                }

                if (updateItem.WaterIntake.HasValue)
                {
                    dailyRecord.UpdateWaterConsumption(new WaterIntake(updateItem.WaterIntake.Value));
                    reportParts.Add($"Su: {updateItem.WaterIntake.Value} ml");
                    dailyUpdateApplied = true;
                }

                if (updateItem.TeacherNote != null)
                {
                    dailyRecord.SetTeacherNote(updateItem.TeacherNote);
                    reportParts.Add($"Not: \"{updateItem.TeacherNote}\"");
                    dailyUpdateApplied = true;
                }

                if (updateItem.IsAbsent.HasValue)
                {
                    dailyRecord.SetAbsentStatus(updateItem.IsAbsent.Value);
                    reportParts.Add(updateItem.IsAbsent.Value ? "Yoklama: Devamsız" : "Yoklama: Derste");
                    if (updateItem.IsAbsent.Value && string.IsNullOrEmpty(dailyRecord.TeacherNote))
                    {
                        dailyRecord.SetTeacherNote("Öğrenci devamsız");
                    }
                    dailyUpdateApplied = true;
                }
            }

            // 2. Yemek Kayıtları (Meal Records) güncellemeleri
            if (updateItem.UpdateMeals && updateItem.Meals != null && updateItem.Meals.Any())
            {
                // Mevcut öğünleri çek
                // (Eğer yeni kayıt ise zaten mevcut öğün yoktur)
                var mealRecords = isNewRecord 
                    ? new List<MealRecord>() 
                    : await _dbContext.MealRecords
                        .Where(m => m.DailyRecordId == dailyRecord.Id && !m.IsDeleted)
                        .ToListAsync(cancellationToken);

                foreach (var mealUpdate in updateItem.Meals)
                {
                    var existingMeal = mealRecords
                        .FirstOrDefault(m => m.MealName.Equals(mealUpdate.MealName, StringComparison.OrdinalIgnoreCase));

                    var mealStatus = new MealStatus((MealStatusType)mealUpdate.StatusType, mealUpdate.StatusDescription ?? string.Empty);

                    if (existingMeal == null)
                    {
                        // Yeni meal record oluştur
                        var plan = weeklyPlans
                            .FirstOrDefault(w => w.MealName.Equals(mealUpdate.MealName, StringComparison.OrdinalIgnoreCase));

                        var newMeal = new MealRecord(dailyRecord.Id, mealUpdate.MealName, mealStatus)
                        {
                            SchoolId = targetStudent.SchoolId
                        };

                        if (plan != null)
                        {
                            newMeal.SetNutrition(plan.PlannedCalories, plan.FoodContent, plan.ProteinGrams, plan.CarbsGrams);
                        }

                        await _dbContext.MealRecords.AddAsync(newMeal, cancellationToken);
                    }
                    else
                    {
                        // Mevcut meal record güncelle
                        existingMeal.UpdateStatus(mealStatus);
                        _dbContext.MealRecords.Update(existingMeal);
                    }

                    reportParts.Add($"{mealUpdate.MealName}: {GetMealStatusText((MealStatusType)mealUpdate.StatusType)}");
                }
            }

            // Eğer herhangi bir güncelleme yapıldıysa
            if (reportParts.Any())
            {
                // Eski AI özetini silerek veli sayfasında yeniden üretilmesini sağla
                var existingSummary = await _dbContext.DailySummaries
                    .FirstOrDefaultAsync(s => s.StudentId == targetStudent.Id && s.Date == targetDate, cancellationToken);
                if (existingSummary != null)
                {
                    _dbContext.DailySummaries.Remove(existingSummary);
                }

                updatedStudentsReport.Add($"{targetStudent.FirstName} {targetStudent.LastName} ({string.Join(", ", reportParts)})");
            }
        }
    }


        // Değişiklikleri kaydet
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AIClassroomUpdateResultDto(
            true, 
            $"{updatedStudentsReport.Count} öğrenci için veriler başarıyla güncellendi.", 
            updatedStudentsReport
        );
    }

    private string GetSleepStatusText(SleepStatus status) => status switch
    {
        SleepStatus.DidNotSleep => "Hiç Uyumadı",
        SleepStatus.SleptLittle => "Az Uyudu",
        SleepStatus.SleptWell => "Çok İyi Uyudu",
        _ => status.ToString()
    };

    private string GetMealStatusText(MealStatusType status) => status switch
    {
        MealStatusType.None => "Hiç Yemedi",
        MealStatusType.Little => "Az Yedi",
        MealStatusType.Half => "Yarısını Yedi",
        MealStatusType.All => "Hepsini Yedi",
        _ => status.ToString()
    };
}

// --- HİLEYLE DESERİLİZE EDEBİLECEĞİMİZ SINIFLAR ---
public class AIUpdateResponse
{
    public List<StudentUpdateItem> Updates { get; set; } = new();
}

public class StudentUpdateItem
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public bool UpdateDailyRecord { get; set; }
    public int? SleepStatus { get; set; }
    public int? WaterIntake { get; set; }
    public string? TeacherNote { get; set; }
    public bool? IsAbsent { get; set; }
    public bool UpdateMeals { get; set; }
    public List<MealUpdateItem>? Meals { get; set; }
}

public class MealUpdateItem
{
    public string MealName { get; set; } = string.Empty;
    public int StatusType { get; set; }
    public string? StatusDescription { get; set; }
}
