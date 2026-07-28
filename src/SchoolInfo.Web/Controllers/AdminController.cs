using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolInfo.Application.Features.Schools.Commands.CreateSchool;
using SchoolInfo.Application.Features.Schools.Commands.UpdateSchool;
using SchoolInfo.Application.Features.Schools.Queries.GetSchool;
using SchoolInfo.Web.Services;

namespace SchoolInfo.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly SchoolInfoApiService _apiService;

    public AdminController(SchoolInfoApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var stats = await _apiService.GetAsync<dynamic>("/api/admin/dashboard-stats");
            return View(stats);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "İstatistikler yüklenirken bir hata oluştu: " + ex.Message;
            return View(null);
        }
    }

    // --- Schools Management ---

    [HttpGet]
    public async Task<IActionResult> Schools()
    {
        try
        {
            var schools = await _apiService.GetAsync<List<SchoolDto>>("/api/admin/schools");
            return View(schools ?? new List<SchoolDto>());
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Okullar yüklenirken hata oluştu: " + ex.Message;
            return View(new List<SchoolDto>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchool(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RedirectToAction(nameof(Schools));

        try
        {
            var command = new CreateSchoolCommand(name);
            await _apiService.PostAsync<dynamic>("/api/admin/schools", command);
            return RedirectToAction(nameof(Schools));
        }
        catch (Exception)
        {
            // Error handling
            return RedirectToAction(nameof(Schools));
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSchool(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RedirectToAction(nameof(Schools));

        try
        {
            var command = new UpdateSchoolCommand(id, name);
            await _apiService.PutAsync($"/api/admin/schools/{id}", command);
            return RedirectToAction(nameof(Schools));
        }
        catch (Exception)
        {
            return RedirectToAction(nameof(Schools));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSchool(Guid id)
    {
        try
        {
            await _apiService.DeleteAsync($"/api/admin/schools/{id}");
            return RedirectToAction(nameof(Schools));
        }
        catch (Exception)
        {
            return RedirectToAction(nameof(Schools));
        }
    }

    // --- Users Management ---

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        try
        {
            var users = await _apiService.GetAsync<List<SchoolInfo.Application.Features.Users.Queries.GetUsers.UserDto>>("/api/admin/users");
            var schools = await _apiService.GetAsync<List<SchoolDto>>("/api/admin/schools");
            ViewBag.Schools = schools ?? new List<SchoolDto>();
            return View(users ?? new List<SchoolInfo.Application.Features.Users.Queries.GetUsers.UserDto>());
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Kullanıcılar yüklenirken hata oluştu: " + ex.Message;
            ViewBag.Schools = new List<SchoolDto>();
            return View(new List<SchoolInfo.Application.Features.Users.Queries.GetUsers.UserDto>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(SchoolInfo.Application.Features.Users.Commands.CreateUser.CreateUserCommand command)
    {
        try
        {
            await _apiService.PostAsync<dynamic>("/api/admin/users", command);
            return RedirectToAction(nameof(Users));
        }
        catch (Exception)
        {
            return RedirectToAction(nameof(Users));
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUser(Guid id, SchoolInfo.Application.Features.Users.Commands.UpdateUser.UpdateUserCommand command)
    {
        if (id != command.Id) return RedirectToAction(nameof(Users));

        try
        {
            await _apiService.PutAsync($"/api/admin/users/{id}", command);
            return RedirectToAction(nameof(Users));
        }
        catch (Exception)
        {
            return RedirectToAction(nameof(Users));
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _apiService.DeleteAsync($"/api/admin/users/{id}");
            return RedirectToAction(nameof(Users));
        }
        catch (Exception)
        {
            return RedirectToAction(nameof(Users));
        }
    }
}
