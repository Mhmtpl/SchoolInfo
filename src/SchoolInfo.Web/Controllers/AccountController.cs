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

            // JWT içindeki claims'leri standart listeye ekliyoruz
            var claims = new List<Claim>();
            foreach (var claim in jsonToken.Claims)
            {
                claims.Add(new Claim(claim.Type, claim.Value));
            }

            // Geriye dönük uyumluluk ve varsayılan claims tanımları
            if (!claims.Any(c => c.Type == ClaimTypes.Name))
            {
                var emailClaim = claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value ?? model.Email;
                claims.Add(new Claim(ClaimTypes.Name, emailClaim));
            }
            if (!claims.Any(c => c.Type == ClaimTypes.Role))
            {
                var roleClaim = claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "Parent";
                claims.Add(new Claim(ClaimTypes.Role, roleClaim));
            }
            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                var subClaim = claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? Guid.NewGuid().ToString();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subClaim));
            }

            claims.Add(new Claim("AccessToken", jwtToken));

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
