using System;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.Events;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Veliye sunulacak günlük özet raporu.
/// </summary>
public class DailySummary : BaseEntity
{
    public Guid StudentId { get; private set; }
    public DateTime Date { get; private set; }
    public string Content { get; private set; }
    public bool IsReadByParent { get; private set; }

    public DailySummary(Guid studentId, DateTime date, string content)
    {
        StudentId = studentId;
        Date = date.Date;
        Content = content;
        IsReadByParent = false;
    }

    /// <summary>
    /// Özeti okundu olarak işaretler.
    /// </summary>
    public void MarkAsRead()
    {
        if (!IsReadByParent)
        {
            IsReadByParent = true;
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Özetin tekrar talep edilmesi olayını fırlatır.
    /// </summary>
    public void RequestSummary()
    {
        AddDomainEvent(new DailySummaryRequestedEvent(Id));
    }
}
