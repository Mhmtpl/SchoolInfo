using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolInfo.Infrastructure.Persistence;

namespace SchoolInfo.API.Endpoints.Auth;

public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);

/// <summary>
/// Kimlik doğrulama işlemleri (Login, Refresh, Logout) için Minimal API endpoint'leri.
/// </summary>
public class AuthEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Kullanıcı girişi yapar, access token ve refresh token döner.")
            .AllowAnonymous()
            .RequireRateLimiting("login");

        group.MapPost("/refresh", RefreshAsync)
            .WithName("Refresh")
            .WithSummary("Refresh token ile yeni access token alır.")
            .AllowAnonymous();

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Refresh token'ı geçersiz kılar.")
            .RequireAuthorization();
    }

    private static async Task<IResult> LoginAsync(
        HttpRequest httpRequest,
        AppDbContext dbContext,
        IConfiguration configuration)
    {
        LoginRequest? request = null;
        try
        {
            httpRequest.EnableBuffering();
            using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            httpRequest.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                request = JsonSerializer.Deserialize<LoginRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch
        {
            request = null;
        }

        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest("E-posta ve şifre gereklidir.");

        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

        if (user == null)
            return Results.Unauthorized();

        // Hem BCrypt doğrulamasını hem de düz metin karşılaştırmasını (fallback) destekliyoruz
        bool isPasswordValid = false;
        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        catch
        {
            // BCrypt format hatası alırsa düz metin kontrolü yap
            isPasswordValid = false;
        }

        if (!isPasswordValid && request.Password == user.PasswordHash)
        {
            isPasswordValid = true;
        }

        if (!isPasswordValid)
            return Results.Unauthorized();

        var accessToken = GenerateAccessToken(user, configuration);
        var refreshToken = GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(30));
        await dbContext.SaveChangesAsync();

        return Results.Ok(new
        {
            Token = accessToken, // Orijinal adlandırmaya (token) geri döndük
            RefreshToken = refreshToken,
            ExpiresIn = 8 * 3600 // saniye
        });
    }

    private static async Task<IResult> RefreshAsync(
        RefreshRequest request,
        AppDbContext dbContext,
        IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Results.BadRequest("Refresh token gereklidir.");

        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && !u.IsDeleted);

        if (user == null)
            return Results.Unauthorized();

        if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Results.Json(
                new { error = "Refresh token süresi dolmuş. Lütfen tekrar giriş yapın." },
                statusCode: 401);

        // Yeni access token ve refresh token üret (token rotation)
        var newAccessToken = GenerateAccessToken(user, configuration);
        var newRefreshToken = GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(30));
        await dbContext.SaveChangesAsync();

        return Results.Ok(new
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 8 * 3600
        });
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        AppDbContext dbContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            var user = await dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.RevokeRefreshToken();
                await dbContext.SaveChangesAsync();
            }
        }
        return Results.NoContent();
    }

    // ─── Yardımcı metodlar ─────────────────────────────────────────────────

    private static string GenerateAccessToken(SchoolInfo.Domain.Entities.User user, IConfiguration configuration)
    {
        var keyString = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key yapılandırması eksik.");
        var key = Encoding.UTF8.GetBytes(keyString);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.ToString()), // Standart uzun URI formatı
            new Claim("role", user.Role.ToString()), // Kısa fallback formatı (çift tırnak engellemek için)
            new Claim("SchoolId", user.SchoolId.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = configuration["Jwt:Issuer"] ?? "SchoolInfo",
            Audience = configuration["Jwt:Audience"] ?? "SchoolInfoUsers",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
