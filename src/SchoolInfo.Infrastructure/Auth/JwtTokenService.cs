using System;

namespace SchoolInfo.Infrastructure.Auth;

/// <summary>
/// Kimlik doÄŸrulama iÅŸlemleri iÃ§in JWT token Ã¼reten servis (Ã–rnek/Taslak).
/// </summary>
public class JwtTokenService
{
    public string GenerateToken(Guid userId, string role)
    {
        // JwtBearer kÃ¼tÃ¼phanesi kullanÄ±larak token Ã¼retimi burada gerÃ§ekleÅŸir.
        return "sample_jwt_token_here";
    }
}
