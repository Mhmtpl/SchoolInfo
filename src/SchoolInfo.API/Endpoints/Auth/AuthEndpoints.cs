using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolInfo.Infrastructure.Persistence;

namespace SchoolInfo.API.Endpoints.Auth;

public record LoginRequest(string Email, string Password);

/// <summary>
/// Kimlik doÄŸrulama iÅŸlemleri (Login, Refresh) iÃ§in Minimal API endpoint'leri.
/// </summary>
public class AuthEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("KullanÄ±cÄ± giriÅŸi yapar ve JWT dÃ¶ner.")
            .AllowAnonymous();

        group.MapPost("/refresh", RefreshAsync)
            .WithName("Refresh")
            .WithSummary("SÃ¼resi dolan token'Ä± yeniler.");
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, AppDbContext dbContext, IConfiguration configuration)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var keyString = configuration["Jwt:Key"] ?? "VerySecretKeyForSchoolInfoApplication123456789";
        var key = Encoding.UTF8.GetBytes(keyString);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("SchoolId", user.SchoolId.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = configuration["Jwt:Issuer"] ?? "SchoolInfo",
            Audience = configuration["Jwt:Audience"] ?? "SchoolInfoUsers",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Results.Ok(new { Token = tokenHandler.WriteToken(token) });
    }

    private static IResult RefreshAsync()
    {
        return Results.Ok(new { Token = "new_dummy_token" });
    }
}
