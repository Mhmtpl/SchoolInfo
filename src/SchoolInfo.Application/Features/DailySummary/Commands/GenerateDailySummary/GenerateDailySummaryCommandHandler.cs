using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolInfo.Application.Common.Interfaces;
using SchoolInfo.Domain.Entities;
using SchoolInfo.Domain.Exceptions;
using SchoolInfo.Domain.Interfaces;

namespace SchoolInfo.Application.Features.DailySummary.Commands.GenerateDailySummary;

/// <summary>
/// GÃ¼nlÃ¼k Ã¶zet raporu oluÅŸturma iÅŸlemini yÃ¼rÃ¼ten sÄ±nÄ±f.
/// </summary>
public class GenerateDailySummaryCommandHandler : IRequestHandler<GenerateDailySummaryCommand, Guid>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IDailyRecordRepository _dailyRecordRepository;
    private readonly IMealRecordRepository _mealRecordRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly IDailySummaryRepository _dailySummaryRepository;
    private readonly IAISummaryService _aiSummaryService;
    private readonly INotificationService _notificationService;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GenerateDailySummaryCommandHandler(
        IStudentRepository studentRepository,
        IDailyRecordRepository dailyRecordRepository,
        IMealRecordRepository mealRecordRepository,
        IActivityRepository activityRepository,
        IDailySummaryRepository dailySummaryRepository,
        IAISummaryService aiSummaryService,
        INotificationService notificationService,
        IAppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _studentRepository = studentRepository;
        _dailyRecordRepository = dailyRecordRepository;
        _mealRecordRepository = mealRecordRepository;
        _activityRepository = activityRepository;
        _dailySummaryRepository = dailySummaryRepository;
        _aiSummaryService = aiSummaryService;
        _notificationService = notificationService;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(GenerateDailySummaryCommand request, CancellationToken cancellationToken)
    {
        // 1. ICurrentUserService ile yetki kontrolü yap (Arka plan servisi çalıştığında role boş gelecektir, buna izin veriyoruz)
        if (_currentUserService.Role != "Teacher" && _currentUserService.Role != "Admin" && _currentUserService.Role != "System" && _currentUserService.Role != "Parent" && _currentUserService.Role != "")
        {
            throw new UnauthorizedAccessException("Günlük özet raporu oluşturmak için yetkiniz bulunmamaktadır.");
        }

        // 2. StudentRepository'den Ã¶ÄŸrenciyi getir
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new StudentNotFoundException(request.StudentId);
        }

        // 3. IDailyRecordRepository'den o gÃ¼nÃ¼n kaydÄ±nÄ± getir
        var dailyRecord = await _dailyRecordRepository.GetByStudentAndDateAsync(request.StudentId, request.Date);
        if (dailyRecord == null)
        {
            throw new DomainException("Bu tarihte Ã¶ÄŸrenciye ait gÃ¼nlÃ¼k kayÄ±t bulunamadÄ±.");
        }

        // 4. IMealRecordRepository'den yemek kayÄ±tlarÄ±nÄ± getir
        var meals = await _mealRecordRepository.GetByDailyRecordIdAsync(dailyRecord.Id);

        // 5. IActivityRepository'den etkinlikleri getir
        var activities = await _activityRepository.GetByClassroomIdAsync(student.ClassroomId, dailyRecord.Date);

        // 5b. Biyometrik verileri veritabanından çek ve ders saatlerine göre eşle
        BiometricSummaryDto? biometricSummary = null;
        var bioRecord = await _dbContext.StudentBiometricRecords
            .FirstOrDefaultAsync(b => b.StudentId == student.Id && b.Date == dailyRecord.Date && !b.IsDeleted, cancellationToken);

        if (bioRecord != null && !string.IsNullOrWhiteSpace(bioRecord.DataJson))
        {
            try
            {
                var points = System.Text.Json.JsonSerializer.Deserialize<List<BiometricPointDto>>(bioRecord.DataJson) ?? new();
                var biometricActivities = new List<BiometricActivitySummaryDto>();

                foreach (var activity in activities)
                {
                    // Aktivite saat aralığındaki nabız verilerini filtrele
                    var matchedPoints = points.Where(p => 
                        TimeSpan.TryParse(p.Time, out var pointTime) && 
                        pointTime >= activity.StartTime && 
                        pointTime <= activity.EndTime
                    ).ToList();

                    var hrValues = matchedPoints.Where(p => p.HeartRate.HasValue).Select(p => p.HeartRate!.Value).ToList();
                    
                    if (hrValues.Any())
                    {
                        biometricActivities.Add(new BiometricActivitySummaryDto(
                            ActivityTitle: activity.Title,
                            ActivityType: activity.Type.ToString(),
                            AvgHeartRate: (int)Math.Round(hrValues.Average()),
                            MinHeartRate: hrValues.Min(),
                            MaxHeartRate: hrValues.Max()
                        ));
                    }
                }

                biometricSummary = new BiometricSummaryDto(
                    OverallAvgHeartRate: bioRecord.AverageHeartRate,
                    OverallAvgSpO2: bioRecord.AverageSpO2,
                    OverallAvgTemp: bioRecord.AverageBodyTemperature,
                    Activities: biometricActivities
                );
            }
            catch (Exception ex)
            {
                // Biyometri okuma hatasında sessizce yoksay (hata toleransı)
            }
        }

        // 6. Verileri SummaryRequestDto'ya map et
        var summaryDto = new SummaryRequestDto(
            StudentName: student.FirstName,
            Date: dailyRecord.Date,
            WaterIntake: new WaterIntakeDto(dailyRecord.WaterConsumption.AmountInMilliliters, dailyRecord.WaterConsumption.AmountInMilliliters > 0),
            Sleep: new SleepDto(
                Minutes: (dailyRecord.SleepInfo.EndTime.HasValue && dailyRecord.SleepInfo.StartTime.HasValue) 
                    ? (int)(dailyRecord.SleepInfo.EndTime.Value - dailyRecord.SleepInfo.StartTime.Value).TotalMinutes 
                    : 0,
                Status: dailyRecord.SleepInfo.Status.ToString()),
            ToiletCount: 0, // VarsayÄ±lan deÄŸer
            Meals: meals.Select(m => new MealDto(m.MealName, m.Status.Type.ToString(), m.Status.Description ?? "")).ToList(),
            Activities: activities.Select(a => new ActivityDto(a.Title, a.Description, a.ActivityDate)).ToList(),
            Biometrics: biometricSummary
        );

        string aiContent;
        if (dailyRecord.IsAbsent)
        {
            aiContent = "Öğrencimiz bugün devamsız olduğu için günlük özet raporu bulunmamaktadır.";
        }
        else
        {
            // 7. IAISummaryService.GenerateAsync() çağır
            aiContent = await _aiSummaryService.GenerateAsync(summaryDto);
        }
        
        // 8. DailySummary entity'si oluştur ve kaydet
        var summary = new SchoolInfo.Domain.Entities.DailySummary(student.Id, request.Date, aiContent);
        summary.SchoolId = student.SchoolId;

        await _dailySummaryRepository.AddAsync(summary);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return summary.Id;
    }
}

internal class BiometricPointDto
{
    [System.Text.Json.Serialization.JsonPropertyName("t")]
    public string Time { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("hr")]
    public int? HeartRate { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("sp")]
    public double? SpO2 { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("temp")]
    public double? BodyTemperature { get; set; }
}
