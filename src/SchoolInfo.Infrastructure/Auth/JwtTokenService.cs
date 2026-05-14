using System;

namespace SchoolInfo.Infrastructure.Auth;

/// <summary>
/// Kimlik doğrulama işlemleri için JWT token üreten servis (Örnek/Taslak).
/// </summary>
public class JwtTokenService
{
    public string GenerateToken(Guid userId, string role)
    {
        // JwtBearer kütüphanesi kullanılarak token üretimi burada gerçekleşir.
        return "sample_jwt_token_here";
    }
}
