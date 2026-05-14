using System;
using System.Collections.Generic;

namespace SchoolInfo.Domain.Common;

/// <summary>
/// Tüm entity'ler için temel sınıf.
/// Kimlik, oluşturulma/güncellenme zamanı ve domain event yönetimini içerir.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Entity'nin benzersiz kimliği.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Multi-tenant (Çoklu Okul) yapısı için okul kimliği.
    /// </summary>
    public Guid SchoolId { get; set; }

    /// <summary>
    /// Entity'nin oluşturulma zamanı.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Entity'nin son güncellenme zamanı.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Entity üzerinde gerçekleşen domain event'leri.
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
    /// Tüm domain event'leri temizler.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Güncellenme zamanını şu anki zamana ayarlar.
    /// </summary>
    protected void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
