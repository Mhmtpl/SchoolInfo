using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolInfo.Web.Models;
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
            var schoolIdClaim = User.FindFirst("SchoolId")?.Value;
            var classrooms = new List<ClassroomDto>();

            // API'de listeleme endpoint'i olmadığı için, okul ID'sine göre 
            // seed edilmiş mevcut sınıfları admin paneline sunuyoruz.
            if (!string.IsNullOrEmpty(schoolIdClaim))
            {
                var parsedSchoolId = Guid.Parse(schoolIdClaim);
                
                // Okul 1 (Papatyalar Sınıfı)
                if (schoolIdClaim.Equals("10b06b00-349f-4318-97be-fb14c330f81d", StringComparison.OrdinalIgnoreCase))
                {
                    classrooms.Add(new ClassroomDto { Id = Guid.Parse("80b06b00-349f-4318-97be-fb14c330f81d"), Name = "Papatyalar Sınıfı", SchoolId = parsedSchoolId });
                    classrooms.Add(new ClassroomDto { Id = Guid.Parse("90b06b00-349f-4318-97be-fb14c330f81d"), Name = "Papatyalar Sınıfı 2", SchoolId = parsedSchoolId });
                }
                // Okul 2 (Menekşeler Sınıfı)
                else if (schoolIdClaim.Equals("20b06b00-349f-4318-97be-fb14c330f81d", StringComparison.OrdinalIgnoreCase))
                {
                    classrooms.Add(new ClassroomDto { Id = Guid.Parse("a0b06b00-349f-4318-97be-fb14c330f81d"), Name = "Menekşeler Sınıfı", SchoolId = parsedSchoolId });
                    classrooms.Add(new ClassroomDto { Id = Guid.Parse("b0b06b00-349f-4318-97be-fb14c330f81d"), Name = "Menekşeler Sınıfı 2", SchoolId = parsedSchoolId });
                }
            }

            return View(classrooms);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Sınıflar yüklenirken bir hata oluştu: " + ex.Message;
            return View(new List<ClassroomDto>());
        }
    }
}
