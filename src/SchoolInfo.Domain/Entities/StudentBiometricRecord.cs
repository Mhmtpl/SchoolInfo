using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Öğrencinin günlük biyometrik sağlık verisi özeti ve detaylı veri noktaları (JSON formatında).
/// </summary>
public class StudentBiometricRecord : BaseEntity
{
    public Guid StudentId { get; private set; }
    public DateTime Date { get; private set; } // Türkiye yerel tarihi (örn: 2026-07-26 00:00:00)
    public string DataJson { get; private set; } = "[]"; // Detaylı ölçüm noktalarını tutan JSON listesi
    public int? AverageHeartRate { get; private set; }
    public double? AverageSpO2 { get; private set; }
    public double? AverageBodyTemperature { get; private set; }
    public DateTime RecordedAt { get; private set; } // Son güncelleme UTC zamanı

    protected StudentBiometricRecord() { }

    public StudentBiometricRecord(
        Guid studentId, 
        DateTime date,
        Guid schoolId)
    {
        StudentId = studentId;
        Date = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        SchoolId = schoolId;
        DataJson = "[]";
        RecordedAt = DateTime.UtcNow;
    }

    public void UpdateData(string dataJson, int? avgHeartRate, double? avgSpO2, double? avgBodyTemp)
    {
        DataJson = dataJson;
        AverageHeartRate = avgHeartRate;
        AverageSpO2 = avgSpO2;
        AverageBodyTemperature = avgBodyTemp;
        RecordedAt = DateTime.UtcNow;
    }
}
