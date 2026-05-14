using System;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Mevcut kullanıcının bilgilerini sağlayan servis.
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid SchoolId { get; }
    string Role { get; }
}
