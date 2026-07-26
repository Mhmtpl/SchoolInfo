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

namespace SchoolInfo.Application.Features.Biometrics.Queries.GetStudentBiometrics;

public record GetStudentBiometricsQuery(Guid StudentId, DateTime? Date, string? Range = null) : IRequest<List<StudentBiometricDto>>;

public record StudentBiometricDto(Guid Id, int? HeartRate, double? SpO2, double? BodyTemperature, DateTime RecordedAt);

public class QueryBiometricDataPoint
{
    [JsonPropertyName("t")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("hr")]
    public int? HeartRate { get; set; }

    [JsonPropertyName("sp")]
    public double? SpO2 { get; set; }

    [JsonPropertyName("temp")]
    public double? BodyTemperature { get; set; }
}

public class GetStudentBiometricsQueryHandler : IRequestHandler<GetStudentBiometricsQuery, List<StudentBiometricDto>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentBiometricsQueryHandler(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<List<StudentBiometricDto>> Handle(GetStudentBiometricsQuery request, CancellationToken cancellationToken)
    {
        // 1. Önce öğrencinin varlığını kontrol et ve Tenant doğrulaması yap
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.SchoolId == _currentUserService.SchoolId && !s.IsDeleted, cancellationToken);

        if (student == null)
        {
            throw new KeyNotFoundException("Öğrenci bulunamadı.");
        }

        // 2. Yetki Kontrolleri (BOLA Önleme)
        if (_currentUserService.Role == "Parent")
        {
            var isParentOfStudent = await _dbContext.Students
                .AnyAsync(s => s.Id == request.StudentId && !s.IsDeleted && s.Parents.Any(p => p.Id == _currentUserService.UserId), cancellationToken);

            if (!isParentOfStudent)
            {
                throw new UnauthorizedAccessException("Bu öğrencinin sağlık verilerine erişim yetkiniz yok.");
            }
        }
        else if (_currentUserService.Role == "Teacher")
        {
            var isAssignedToClass = await _dbContext.Classrooms
                .AnyAsync(c => c.Id == student.ClassroomId && !c.IsDeleted && c.Teachers.Any(t => t.Id == _currentUserService.UserId), cancellationToken);

            if (!isAssignedToClass)
            {
                throw new UnauthorizedAccessException("Sorumluluğunuzda olmayan bir sınıfın öğrencisinin verilerine erişemezsiniz.");
            }
        }
        else if (_currentUserService.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");
        }

        // 3. Tarih Aralığı (Range) Sorgusu Varsa: Günlük Ortalamaları Getir
        if (!string.IsNullOrEmpty(request.Range))
        {
            int days = request.Range == "30days" ? 30 : 7;
            var startLocal = DateTime.UtcNow.AddHours(3).Date.AddDays(-days);
            var endLocal = DateTime.UtcNow.AddHours(3).Date.AddDays(1); // Yarına kadar
            var startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(endLocal, DateTimeKind.Utc);

            // Günlük kayıtları doğrudan pre-calculated alanlardan listele (Milyonlarca veri taramadan ultra hızlı)
            var raw = await _dbContext.StudentBiometricRecords
                .Where(r => r.StudentId == request.StudentId && 
                            r.SchoolId == _currentUserService.SchoolId &&
                            r.Date >= startUtc && 
                            r.Date < endUtc && 
                            !r.IsDeleted)
                .OrderBy(r => r.Date)
                .ToListAsync(cancellationToken);

            var list = raw
                .Select(r => new StudentBiometricDto(
                    r.Id,
                    r.AverageHeartRate,
                    r.AverageSpO2,
                    r.AverageBodyTemperature,
                    DateTime.SpecifyKind(r.Date, DateTimeKind.Utc)
                ))
                .ToList();

            return list;
        }

        // 4. Tek bir tarihteki biyometrik kayıtları getir (Canlı akış grafiği için)
        var targetDate = (request.Date ?? DateTime.UtcNow.AddHours(3).Date).Date;
        var targetDateUtc = DateTime.SpecifyKind(targetDate, DateTimeKind.Utc);

        var dailyRecord = await _dbContext.StudentBiometricRecords
            .FirstOrDefaultAsync(r => r.StudentId == request.StudentId && 
                                     r.SchoolId == _currentUserService.SchoolId &&
                                     r.Date == targetDateUtc && 
                                     !r.IsDeleted, 
                                 cancellationToken);

        if (dailyRecord == null || string.IsNullOrEmpty(dailyRecord.DataJson))
        {
            return new List<StudentBiometricDto>();
        }

        // JSON dizisini çöz ve DTO listesine dönüştür
        var points = JsonSerializer.Deserialize<List<QueryBiometricDataPoint>>(dailyRecord.DataJson) 
                     ?? new List<QueryBiometricDataPoint>();

        var records = new List<StudentBiometricDto>();
        foreach (var point in points)
        {
            if (TimeSpan.TryParse(point.Time, out var timeOffset))
            {
                // Türkiye yerel tarihi ve saatini birleştir
                var localTime = targetDate.Add(timeOffset);
                // UTC zaman damgasına çevir (Örn: +3 saat çıkararak)
                var recordedAtUtc = DateTime.SpecifyKind(localTime.AddHours(-3), DateTimeKind.Utc);

                records.Add(new StudentBiometricDto(
                    dailyRecord.Id,
                    point.HeartRate,
                    point.SpO2,
                    point.BodyTemperature,
                    recordedAtUtc
                ));
            }
        }

        return records.OrderBy(r => r.RecordedAt).ToList();
    }
}
