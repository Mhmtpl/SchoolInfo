using MediatR;

namespace SchoolInfo.Domain.Common;

/// <summary>
/// Domain event'leri işaretlemek için kullanılan arayüz.
/// </summary>
public interface IDomainEvent : INotification
{
}
