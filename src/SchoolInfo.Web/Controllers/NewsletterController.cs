using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolInfo.Web.Models;
using SchoolInfo.Web.Services;

namespace SchoolInfo.Web.Controllers;

[Authorize(Roles = "Teacher,Parent")]
public class NewsletterController : Controller
{
    private readonly SchoolInfoApiService _apiService;

    public NewsletterController(SchoolInfoApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> CreateNewsletter([FromForm] CreateNewsletterCommand command, Microsoft.AspNetCore.Http.IFormFile? CoverImage)
    {
        try
        {
            if (CoverImage != null && CoverImage.Length > 0)
            {
                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "newsletters");
                System.IO.Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + CoverImage.FileName;
                var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await CoverImage.CopyToAsync(fileStream);
                }
                command.ImageUrl = "/uploads/newsletters/" + uniqueFileName;
            }

            await _apiService.PostAsync("api/newsletters", command);
            return RedirectToAction("ClassroomDetails", "Teacher", new { id = command.ClassroomId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Bülten oluşturulurken hata oluştu: " + ex.Message;
            return RedirectToAction("ClassroomDetails", "Teacher", new { id = command.ClassroomId });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> PublishNewsletter(Guid id, Guid classroomId)
    {
        try
        {
            await _apiService.PutAsync<object>($"api/newsletters/{id}/publish", new { });
            return RedirectToAction("ClassroomDetails", "Teacher", new { id = classroomId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Bülten yayınlanırken hata oluştu: " + ex.Message;
            return RedirectToAction("ClassroomDetails", "Teacher", new { id = classroomId });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> UpdateNewsletter(Guid id, [FromForm] CreateNewsletterCommand command, Microsoft.AspNetCore.Http.IFormFile? CoverImage)
    {
        try
        {
            if (CoverImage != null && CoverImage.Length > 0)
            {
                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "newsletters");
                System.IO.Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + CoverImage.FileName;
                var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await CoverImage.CopyToAsync(fileStream);
                }
                command.ImageUrl = "/uploads/newsletters/" + uniqueFileName;
            }

            await _apiService.PutAsync($"api/newsletters/{id}", command);
            return RedirectToAction("ClassroomDetails", "Teacher", new { id = command.ClassroomId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Bülten güncellenirken hata oluştu: " + ex.Message;
            return RedirectToAction("ClassroomDetails", "Teacher", new { id = command.ClassroomId });
        }
    }

    [HttpDelete("api/newsletters/{id}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteNewsletter(Guid id)
    {
        try
        {
            await _apiService.DeleteAsync($"api/newsletters/{id}");
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> DownloadPdf(Guid id)
    {
        try
        {
            var pdfBytes = await _apiService.GetByteArrayAsync($"api/newsletters/{id}/pdf");
            return File(pdfBytes, "application/pdf", $"bulten_{id}.pdf");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Bülten indirilirken hata oluştu: " + ex.Message;
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
