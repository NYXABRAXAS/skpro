using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SuryodaySelfKiosk.Configuration;
using SuryodaySelfKiosk.Services;
using SuryodaySelfKiosk.ViewModels;

namespace SuryodaySelfKiosk.Controllers;

/// <summary>The large touchscreen home screen shown on the physical kiosk.</summary>
public class KioskController(IQrCodeService qr, IOptions<SelfKioskOptions> options) : Controller
{
    private readonly SelfKioskOptions _cfg = options.Value;

    [HttpGet("/")]
    [HttpGet("/kiosk")]
    public IActionResult Index()
    {
        var vm = new KioskHomeViewModel
        {
            JourneyUrl = _cfg.JourneyUrl,
            QrDataUri = qr.GetQrDataUri(_cfg.JourneyUrl, pixelsPerModule: 12),
            BankName = _cfg.BankName,
            ProductName = _cfg.ProductName,
            MockMode = _cfg.MockMode
        };
        return View(vm);
    }
}
