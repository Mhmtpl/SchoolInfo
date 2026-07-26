using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SchoolInfo.Web.Models;

namespace SchoolInfo.Web.Services;

/// <summary>
/// SchoolInfo.API arka uç servisi ile güvenli ve yetkilendirilmiş HTTP haberleşmesini yönetir.
/// </summary>
public class SchoolInfoApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public string ApiUrl => _httpClient.BaseAddress?.ToString() ?? "";
    public string PublicApiUrl { get; }

    public SchoolInfoApiService(
        HttpClient httpClient, 
        IHttpContextAccessor httpContextAccessor, 
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;

        // Base URL'i appsettings.json'dan çekiyoruz
        var apiUrl = configuration["SchoolInfoApiUrl"] ?? "http://localhost:53079";
        _httpClient.BaseAddress = new Uri(apiUrl.TrimEnd('/') + "/");

        // Tarayıcının dışarıdan erişebileceği kamuya açık API URL'i
        PublicApiUrl = configuration["PublicApiUrl"] ?? apiUrl;
    }

    private async Task AttachAuthorizationHeaderAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;
        var tokenClaim = user?.FindFirst("AccessToken")?.Value;

        if (user?.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(tokenClaim))
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            if (jwtHandler.CanReadToken(tokenClaim))
            {
                var jwtToken = jwtHandler.ReadJwtToken(tokenClaim);
                // Eğer access token süresi dolmuşsa veya 5 dakikadan az kalmışsa yenile
                if (jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(5))
                {
                    var refreshed = await RefreshTokensAsync();
                    if (refreshed && httpContext != null)
                    {
                        tokenClaim = httpContext.User.FindFirst("AccessToken")?.Value;
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(tokenClaim))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenClaim);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    private async Task<bool> RefreshTokensAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return false;

        var user = httpContext.User;
        var refreshToken = user.FindFirst("RefreshToken")?.Value;
        if (string.IsNullOrEmpty(refreshToken)) return false;

        try
        {
            // API'ye refresh isteği atıyoruz. Authorization header eklememek için doğrudan PostAsJsonAsync çağrısı yapıyoruz.
            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new { RefreshToken = refreshToken });
            if (!response.IsSuccessStatusCode)
            {
                // Refresh token geçersiz ise kullanıcının oturumunu kapat
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return false;
            }

            var refreshResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (refreshResult == null || string.IsNullOrEmpty(refreshResult.Token)) return false;

            var identity = user.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var oldAccessToken = identity.FindFirst("AccessToken");
                if (oldAccessToken != null) identity.RemoveClaim(oldAccessToken);

                var oldRefreshToken = identity.FindFirst("RefreshToken");
                if (oldRefreshToken != null) identity.RemoveClaim(oldRefreshToken);

                identity.AddClaim(new Claim("AccessToken", refreshResult.Token));
                identity.AddClaim(new Claim("RefreshToken", refreshResult.RefreshToken));

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);
                return true;
            }
        }
        catch
        {
            // ignore
        }
        return false;
    }

    // --- GENEL HTTP METOTLARI ---

    public async Task<T?> GetAsync<T>(string relativeUri)
    {
        await AttachAuthorizationHeaderAsync();
        var response = await _httpClient.GetAsync(relativeUri);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<byte[]> GetByteArrayAsync(string relativeUri)
    {
        await AttachAuthorizationHeaderAsync();
        var response = await _httpClient.GetAsync(relativeUri);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string relativeUri, TRequest payload)
    {
        await AttachAuthorizationHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(relativeUri, payload);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task PostAsync<TRequest>(string relativeUri, TRequest payload)
    {
        await AttachAuthorizationHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(relativeUri, payload);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
    }

    public async Task PutAsync<TRequest>(string relativeUri, TRequest payload)
    {
        await AttachAuthorizationHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync(relativeUri, payload);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string relativeUri)
    {
        await AttachAuthorizationHeaderAsync();
        var response = await _httpClient.DeleteAsync(relativeUri);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
    }

    // --- AUTHENTICATION SPECIFIC ---

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginModel { Email = email, Password = password });
        
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("E-posta veya şifre hatalı.");

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }
}
