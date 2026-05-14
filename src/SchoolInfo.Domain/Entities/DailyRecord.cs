using System;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.ValueObjects;
using SchoolInfo.Domain.Events;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Ã–ÄŸrencinin gÃ¼nlÃ¼k kaydÄ±.
/// </summary>
public class DailyRecord : BaseEntity
{
    public Guid StudentId { get; private set; }
    public DateTime Date { get; private set; }
    
    public SleepData SleepInfo { get; private set; }
    public WaterIntake WaterConsumption { get; private set; }
    public string? TeacherNote { get; private set; }

    public DailyRecord(Guid studentId, DateTime date)
    {
        StudentId = studentId;
        Date = date.Date;
        SleepInfo = new SleepData(Enums.SleepStatus.DidNotSleep, null, null);
        WaterConsumption = new WaterIntake(0);

        AddDomainEvent(new DailyRecordCreatedEvent(Id, StudentId));
    }

    /// <summary>
    /// Uyku bilgisini gÃ¼nceller.
    /// </summary>
    public void UpdateSleepInfo(SleepData sleepData)
    {
        SleepInfo = sleepData;
        UpdateTimestamp();
    }

    /// <summary>
    /// Su tÃ¼ketimini gÃ¼nceller.
    /// </summary>
    public void UpdateWaterConsumption(WaterIntake waterIntake)
    {
        WaterConsumption = waterIntake;
        UpdateTimestamp();
    }

    /// <summary>
    /// Ã–ÄŸretmen notu ekler veya gÃ¼nceller.
    /// </summary>
    public void SetTeacherNote(string note)
    {
        TeacherNote = note;
        UpdateTimestamp();
    }
}
