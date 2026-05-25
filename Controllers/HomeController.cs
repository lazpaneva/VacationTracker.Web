using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VacationTracker.Web.Models;

namespace VacationTracker.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Employees", new { year = DateTime.Today.Year });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
