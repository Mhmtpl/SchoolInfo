using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
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
    }

    private void AttachAuthorizationHeader()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var tokenClaim = user?.FindFirst("AccessToken")?.Value;

        System.Console.WriteLine($"[DIAGNOSTIC] tokenClaim degeri: '{tokenClaim}'");
        if (user?.Identity?.IsAuthenticated == true)
        {
            System.Console.WriteLine("[DIAGNOSTIC] Mevcut Claims:");
            foreach (var c in user.Claims)
            {
                System.Console.WriteLine($"  Type: '{c.Type}', Value: '{c.Value}'");
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

    // --- GENEL HTTP METOTLARI ---

    public async Task<T?> GetAsync<T>(string relativeUri)
    {
        AttachAuthorizationHeader();
        var response = await _httpClient.GetAsync(relativeUri);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string relativeUri, TRequest payload)
    {
        AttachAuthorizationHeader();
        var response = await _httpClient.PostAsJsonAsync(relativeUri, payload);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task PostAsync<TRequest>(string relativeUri, TRequest payload)
    {
        AttachAuthorizationHeader();
        var response = await _httpClient.PostAsJsonAsync(relativeUri, payload);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
    }

    public async Task PutAsync<TRequest>(string relativeUri, TRequest payload)
    {
        AttachAuthorizationHeader();
        var response = await _httpClient.PutAsJsonAsync(relativeUri, payload);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string relativeUri)
    {
        AttachAuthorizationHeader();
        var response = await _httpClient.DeleteAsync(relativeUri);

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz bulunmamaktadır.");

        response.EnsureSuccessStatusCode();
    }

    // --- AUTHENTICATION SPECIFIC ---

    public async Task<string> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginModel { Email = email, Password = password });
        
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("E-posta veya şifre hatalı.");

        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResult?.Token ?? string.Empty;
    }
}
