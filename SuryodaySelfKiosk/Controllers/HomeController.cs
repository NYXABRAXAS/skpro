using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SuryodaySelfKiosk.Models;

namespace SuryodaySelfKiosk.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Kiosk");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
