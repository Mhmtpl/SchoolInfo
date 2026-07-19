using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Öğrencinin biyometrik sağlık verileri (Nabız, SpO2, Ateş).
/// </summary>
public class StudentBiometricRecord : BaseEntity
{
    public Guid StudentId { get; private set; }
    public int? HeartRate { get; private set; }
    public double? SpO2 { get; private set; }
    public double? BodyTemperature { get; private set; }
    public DateTime RecordedAt { get; private set; }

    protected StudentBiometricRecord() { }

    public StudentBiometricRecord(
        Guid studentId, 
        int? heartRate, 
        double? spO2, 
        double? bodyTemperature, 
        DateTime recordedAt,
        Guid schoolId)
    {
        StudentId = studentId;
        HeartRate = heartRate;
        SpO2 = spO2;
        BodyTemperature = bodyTemperature;
        RecordedAt = DateTime.SpecifyKind(recordedAt, DateTimeKind.Utc);
        SchoolId = schoolId;
    }
}
