using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolInfo.Application.Common.Interfaces;

namespace SchoolInfo.API.Services;

/// <summary>
/// Sistemde o an işlem yapan kullanıcıyı HTTP Context içerisindeki JWT Token'dan (Claims) okuyan servis.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(id) ? Guid.Empty : Guid.Parse(id);
        }
    }

    public Guid SchoolId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User?.FindFirst("SchoolId")?.Value;
            return string.IsNullOrEmpty(id) ? Guid.Empty : Guid.Parse(id);
        }
    }

    public string Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
