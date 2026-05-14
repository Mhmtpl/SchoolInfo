using System;
using System.Collections.Generic;

namespace SchoolInfo.Domain.Common;

/// <summary>
/// TÃ¼m entity'ler iÃ§in temel sÄ±nÄ±f.
/// Kimlik, oluÅŸturulma/gÃ¼ncellenme zamanÄ± ve domain event yÃ¶netimini iÃ§erir.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Entity'nin benzersiz kimliÄŸi.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Multi-tenant (Ã‡oklu Okul) yapÄ±sÄ± iÃ§in okul kimliÄŸi.
    /// </summary>
    public Guid SchoolId { get; set; }

    /// <summary>
    /// Entity'nin oluÅŸturulma zamanÄ±.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Entity'nin son gÃ¼ncellenme zamanÄ±.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Entity'nin silinme durumu (Soft Delete).
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Entity'nin silinme zamanÄ±.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Entity Ã¼zerinde gerÃ§ekleÅŸen domain event'leri.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Yeni bir domain event ekler.
    /// </summary>
    /// <param name="domainEvent">Eklenecek olay</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// TÃ¼m domain event'leri temizler.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// GÃ¼ncellenme zamanÄ±nÄ± ÅŸu anki zamana ayarlar.
    /// </summary>
    protected void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Entity'i silindi olarak iÅŸaretler (Soft Delete).
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }
}
