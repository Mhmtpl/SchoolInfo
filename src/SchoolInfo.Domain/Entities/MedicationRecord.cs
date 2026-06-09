using System;
using SchoolInfo.Domain.Common;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Öğrencinin günlük ilaç uygulama kaydı.
/// </summary>
public class MedicationRecord : BaseEntity
{
    public Guid DailyRecordId { get; private set; }
    public Guid StudentId { get; private set; }
    public string MedicineName { get; private set; }
    public string Dosage { get; private set; }
    public string AdministrationTime { get; private set; }
    public bool Taken { get; private set; }
    public string? Note { get; private set; }

    protected MedicationRecord() { }

    public MedicationRecord(
        Guid dailyRecordId,
        Guid studentId,
        string medicineName,
        string dosage,
        string administrationTime,
        bool taken,
        string? note)
    {
        DailyRecordId = dailyRecordId;
        StudentId = studentId;
        MedicineName = medicineName;
        Dosage = dosage;
        AdministrationTime = administrationTime;
        Taken = taken;
        Note = note;
    }

    public void UpdateDetails(
        string medicineName,
        string dosage,
        string administrationTime,
        bool taken,
        string? note)
    {
        MedicineName = medicineName;
        Dosage = dosage;
        AdministrationTime = administrationTime;
        Taken = taken;
        Note = note;
        UpdateTimestamp();
    }
}
