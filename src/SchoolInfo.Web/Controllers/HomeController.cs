using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolInfo.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
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

        return RedirectToAction("Logout", "Account");
    }
}
