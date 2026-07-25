using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolInfo.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (User.IsInRole("Teacher"))
            {
                return RedirectToAction("Index", "Teacher");
            }
            else if (User.IsInRole("Parent"))
            {
                return RedirectToAction("Index", "Parent");
            }
        }

        // Show the promotional landing page for anonymous users
        return View();
    }
}
