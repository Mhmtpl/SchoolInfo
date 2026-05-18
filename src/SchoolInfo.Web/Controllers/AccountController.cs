using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SchoolInfo.Web.Models;
using SchoolInfo.Web.Services;

namespace SchoolInfo.Web.Controllers;

public class AccountController : Controller
{
    private readonly SchoolInfoApiService _apiService;

    public AccountController(SchoolInfoApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new LoginModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // API üzerinden giriş yapıp JWT Token alıyoruz
            var jwtToken = await _apiService.LoginAsync(model.Email, model.Password);

            if (string.IsNullOrEmpty(jwtToken))
            {
                ModelState.AddModelError(string.Empty, "Kimlik doğrulama başarısız.");
                return View(model);
            }

            // JWT Token'ı parse edip içindeki claim'leri okuyoruz
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

            if (jsonToken == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kimlik belgesi.");
                return View(model);
            }

            // Helper to get claim value safely from both short names and long XML URIs
            Func<string, string, string, string> getClaim = (shortName, longUri, defVal) =>
            {
                if (jsonToken.Payload.TryGetValue(shortName, out var v1) && v1 != null) return v1.ToString()!;
                if (jsonToken.Payload.TryGetValue(longUri, out var v2) && v2 != null) return v2.ToString()!;
                return defVal;
            };

            var emailVal = getClaim("email", ClaimTypes.Email, model.Email);
            var roleVal = getClaim("role", ClaimTypes.Role, "Parent");
            var nameIdVal = getClaim("sub", ClaimTypes.NameIdentifier, Guid.NewGuid().ToString());
            var schoolIdVal = getClaim("SchoolId", "SchoolId", Guid.Empty.ToString());

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, emailVal),
                new Claim(ClaimTypes.NameIdentifier, nameIdVal),
                new Claim(ClaimTypes.Role, roleVal),
                new Claim("SchoolId", schoolIdVal),
                new Claim("AccessToken", jwtToken)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
