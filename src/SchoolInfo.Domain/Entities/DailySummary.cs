using System;
using SchoolInfo.Domain.Common;
using SchoolInfo.Domain.Events;

namespace SchoolInfo.Domain.Entities;

/// <summary>
/// Veliye sunulacak gÃ¼nlÃ¼k Ã¶zet raporu.
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
    /// Ã–zeti okundu olarak iÅŸaretler.
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
    /// Ã–zetin tekrar talep edilmesi olayÄ±nÄ± fÄ±rlatÄ±r.
    /// </summary>
    public void RequestSummary()
    {
        AddDomainEvent(new DailySummaryRequestedEvent(Id));
    }
}
