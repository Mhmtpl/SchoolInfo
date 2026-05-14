using System;

namespace SchoolInfo.Application.Common.Interfaces;

/// <summary>
/// Mevcut kullanÄ±cÄ±nÄ±n bilgilerini saÄŸlayan servis.
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid SchoolId { get; }
    string Role { get; }
}
