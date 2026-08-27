using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SuryodaySelfKiosk.Configuration;
using SuryodaySelfKiosk.Models;
using SuryodaySelfKiosk.Services;
using SuryodaySelfKiosk.Services.Interfaces;
using SuryodaySelfKiosk.ViewModels;

namespace SuryodaySelfKiosk.Controllers;

[Route("car-loan")]
public class CarLoanController : Controller
{
    private readonly SelfKioskOptions _cfg;
    private readonly ApplicationStateService _state;
    private readonly IOtpService _otp;
    private readonly IAadhaarService _aadhaar;
    private readonly IPanService _pan;
    private readonly IBureauService _bureau;
    private readonly IBreService _bre;
    private readonly IBankEmployeeService _employee;
    private readonly ILosService _los;
    private readonly IAuditService _audit;

    public CarLoanController(
        IOptions<SelfKioskOptions> options,
        ApplicationStateService state,
        IOtpService otp,
        IAadhaarService aadhaar,
        IPanService pan,
        IBureauService bureau,
        IBreService bre,
        IBankEmployeeService employee,
        ILosService los,
        IAuditService audit)
    {
        _cfg = options.Value;
        _state = state;
        _otp = otp;
        _aadhaar = aadhaar;
        _pan = pan;
        _bureau = bureau;
        _bre = bre;
        _employee = employee;
        _los = los;
        _audit = audit;
    }

    // --------------------------------------------------------------------
    // Helpers
    // --------------------------------------------------------------------
    private StepViewModel Build(CarLoanApplication app, string step) => new()
    {
        App = app,
        CurrentStep = step,
        MockMode = _cfg.MockMode,
        MockOtp = _cfg.MockMode ? _cfg.MockOtp : null,
        MockEmployeeId = _cfg.MockMode ? _cfg.MockEmployeeId : null,
        OtpExpirySeconds = _cfg.OtpExpirySeconds,
        MaxOtpResendAttempts = _cfg.MaxOtpResendAttempts,
        SessionTimeoutSeconds = _cfg.SessionTimeoutSeconds
    };

    private static string MaskMobile(string mobile) =>
        mobile.Length == 10 ? $"XXXXXX{mobile[^4..]}" : "XXXXXXXXXX";

    // --------------------------------------------------------------------
    // 1. Mobile landing page (QR target)
    // --------------------------------------------------------------------
    [HttpGet("start")]
    public IActionResult Start()
    {
        var app = _state.GetOrCreate();
        _audit.QrScanned(app.ApplicationId);
        return View(Build(app, JourneySteps.Start));
    }

    [HttpPost("start")]
    [ValidateAntiForgeryToken]
    public IActionResult StartSelect(string vehicleType)
    {
        var app = _state.GetOrCreate();
        if (vehicleType is VehicleTypes.New or VehicleTypes.Used)
            app.VehicleType = vehicleType;
        _state.Save(app);
        return RedirectToAction(nameof(Consent));
    }

    // --------------------------------------------------------------------
    // 2. Consent
    // --------------------------------------------------------------------
    [HttpGet("consent")]
    public IActionResult Consent()
    {
        var app = _state.GetOrCreate();
        var vm = Build(app, JourneySteps.Consent);
        vm.Consent = new ConsentInput { AadhaarConsent = app.AadhaarConsent, BureauConsent = app.BureauConsent };
        return View(vm);
    }

    [HttpPost("consent")]
    [ValidateAntiForgeryToken]
    public IActionResult Consent(ConsentInput input)
    {
        var app = _state.GetOrCreate();
        if (!ModelState.IsValid)
        {
            var vm = Build(app, JourneySteps.Consent);
            vm.Consent = input;
            return View(vm);
        }

        app.AadhaarConsent = input.AadhaarConsent;
        app.BureauConsent = input.BureauConsent;
        app.ConsentCapturedAtUtc = DateTimeOffset.UtcNow;
        app.JourneyStep = JourneySteps.Aadhaar;
        _state.Save(app);
        _audit.ConsentCaptured(app.ApplicationId, app.AadhaarConsent, app.BureauConsent);
        return RedirectToAction(nameof(Aadhaar));
    }

    // --------------------------------------------------------------------
    // 4. Mobile + OTP
    // --------------------------------------------------------------------
    [HttpGet("mobile")]
    public IActionResult Mobile()
    {
        var app = _state.GetOrCreate();
        if (!app.AadhaarVerified) return RedirectToAction(nameof(Aadhaar));

        var vm = Build(app, JourneySteps.Mobile);
        vm.Mobile = new MobileInput { MobileNumber = app.MobileNumber };
        return View(vm);
    }

    [HttpPost("mobile/send")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOtp(MobileInput input)
    {
        var app = _state.GetOrCreate();
        if (!ModelState.IsValid)
        {
            var vm = Build(app, JourneySteps.Mobile);
            vm.Mobile = input;
            return View(nameof(Mobile), vm);
        }

        var result = await _otp.SendOtpAsync(input.MobileNumber);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.CustomerMessage ?? "Unable to send OTP. Please try again.");
            var vm = Build(app, JourneySteps.Mobile);
            vm.Mobile = input;
            return View(nameof(Mobile), vm);
        }

        app.MobileNumber = input.MobileNumber;
        app.MobileVerified = false;
        app.OtpSent = true;
        app.OtpSentAtUtc = DateTimeOffset.UtcNow;
        app.OtpResendCount = 0;
        _state.Save(app);
        return RedirectToAction(nameof(Mobile));
    }

    [HttpPost("mobile/change")]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeMobile()
    {
        var app = _state.GetOrCreate();
        app.OtpSent = false;
        app.MobileVerified = false;
        app.OtpResendCount = 0;
        app.OtpSentAtUtc = null;
        _state.Save(app);
        return RedirectToAction(nameof(Mobile));
    }

    [HttpPost("mobile/resend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp()
    {
        var app = _state.GetOrCreate();
        if (!app.OtpSent || string.IsNullOrEmpty(app.MobileNumber))
            return RedirectToAction(nameof(Mobile));

        if (app.OtpResendCount >= _cfg.MaxOtpResendAttempts)
        {
            TempData["OtpError"] = "You have reached the maximum number of OTP resend attempts. Please start again.";
            return RedirectToAction(nameof(Mobile));
        }

        await _otp.SendOtpAsync(app.MobileNumber);
        app.OtpResendCount++;
        app.OtpSentAtUtc = DateTimeOffset.UtcNow;
        _state.Save(app);
        return RedirectToAction(nameof(Mobile));
    }

    [HttpPost("mobile/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(OtpInput input)
    {
        var app = _state.GetOrCreate();
        var vm = Build(app, JourneySteps.Mobile);
        vm.Otp = input;

        if (!app.OtpSent)
            return RedirectToAction(nameof(Mobile));

        if (!ModelState.IsValid)
            return View(nameof(Mobile), vm);

        var expiry = app.OtpSentAtUtc?.AddSeconds(_cfg.OtpExpirySeconds);
        if (expiry is null || DateTimeOffset.UtcNow > expiry)
        {
            ModelState.AddModelError(string.Empty, "OTP has expired. Please request a new OTP.");
            return View(nameof(Mobile), vm);
        }

        var result = await _otp.VerifyOtpAsync(app.MobileNumber, input.Otp);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.CustomerMessage ?? "Invalid OTP. Please try again.");
            return View(nameof(Mobile), vm);
        }

        app.MobileVerified = true;
        app.JourneyStep = JourneySteps.Pan;
        _state.Save(app);
        _audit.OtpVerified(app.ApplicationId, MaskMobile(app.MobileNumber));
        return RedirectToAction(nameof(Pan));
    }

    // --------------------------------------------------------------------
    // 3. Aadhaar eKYC — enter Aadhaar -> OTP to Aadhaar-linked mobile -> verify OTP
    // --------------------------------------------------------------------
    [HttpGet("aadhaar")]
    public IActionResult Aadhaar()
    {
        var app = _state.GetOrCreate();
        if (!app.AadhaarConsent || !app.BureauConsent) return RedirectToAction(nameof(Consent));
        return View(Build(app, JourneySteps.Aadhaar));
    }

    [HttpPost("aadhaar/send")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendAadhaarOtp(AadhaarInput input)
    {
        var app = _state.GetOrCreate();
        if (!ModelState.IsValid)
        {
            var vm = Build(app, JourneySteps.Aadhaar);
            return View(nameof(Aadhaar), vm); // never echo the entered Aadhaar back
        }

        var result = await _aadhaar.SendAadhaarOtpAsync(input.AadhaarNumber);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.CustomerMessage ?? "Aadhaar authentication failed. Please try again.");
            return View(nameof(Aadhaar), Build(app, JourneySteps.Aadhaar));
        }

        // Store only the last 4 digits + mask. The full Aadhaar is never persisted.
        app.AadhaarLast4 = input.AadhaarNumber[^4..];
        app.AadhaarMasked = $"XXXX XXXX {app.AadhaarLast4}";
        app.RegisteredMobileMasked = result.Data ?? $"XXXXXX{app.AadhaarLast4}";
        app.AadhaarVerified = false;
        app.AadhaarOtpSent = true;
        app.AadhaarOtpSentAtUtc = DateTimeOffset.UtcNow;
        app.AadhaarOtpResendCount = 0;
        _state.Save(app);
        return RedirectToAction(nameof(Aadhaar));
    }

    [HttpPost("aadhaar/resend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendAadhaarOtp()
    {
        var app = _state.GetOrCreate();
        if (!app.AadhaarOtpSent) return RedirectToAction(nameof(Aadhaar));

        if (app.AadhaarOtpResendCount >= _cfg.MaxOtpResendAttempts)
        {
            TempData["OtpError"] = "You have reached the maximum number of OTP resend attempts. Please start again.";
            return RedirectToAction(nameof(Aadhaar));
        }

        await _aadhaar.SendAadhaarOtpAsync(new string('0', 8) + app.AadhaarLast4);
        app.AadhaarOtpResendCount++;
        app.AadhaarOtpSentAtUtc = DateTimeOffset.UtcNow;
        _state.Save(app);
        return RedirectToAction(nameof(Aadhaar));
    }

    [HttpPost("aadhaar/change")]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeAadhaar()
    {
        var app = _state.GetOrCreate();
        app.AadhaarOtpSent = false;
        app.AadhaarVerified = false;
        app.AadhaarOtpResendCount = 0;
        app.AadhaarOtpSentAtUtc = null;
        _state.Save(app);
        return RedirectToAction(nameof(Aadhaar));
    }

    [HttpPost("aadhaar/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyAadhaarOtp(OtpInput input)
    {
        var app = _state.GetOrCreate();
        var vm = Build(app, JourneySteps.Aadhaar);
        vm.Otp = input;

        if (!app.AadhaarOtpSent) return RedirectToAction(nameof(Aadhaar));
        if (!ModelState.IsValid) return View(nameof(Aadhaar), vm);

        var expiry = app.AadhaarOtpSentAtUtc?.AddSeconds(_cfg.OtpExpirySeconds);
        if (expiry is null || DateTimeOffset.UtcNow > expiry)
        {
            ModelState.AddModelError(string.Empty, "OTP has expired. Please request a new OTP.");
            return View(nameof(Aadhaar), vm);
        }

        var result = await _aadhaar.VerifyAadhaarOtpAsync(app.AadhaarLast4, input.Otp);
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.CustomerMessage ?? "Aadhaar authentication failed. Please try again.");
            return View(nameof(Aadhaar), vm);
        }

        app.AadhaarVerified = true;
        app.AadhaarMasked = result.Data.AadhaarMasked;
        app.CustomerName = result.Data.CustomerName;
        app.DateOfBirth = result.Data.DateOfBirth;
        app.Address = result.Data.Address;
        app.JourneyStep = JourneySteps.Mobile;
        _state.Save(app);
        _audit.AadhaarVerified(app.ApplicationId);
        return RedirectToAction(nameof(Mobile));
    }

    // --------------------------------------------------------------------
    // 5. PAN
    // --------------------------------------------------------------------
    [HttpGet("pan")]
    public IActionResult Pan()
    {
        var app = _state.GetOrCreate();
        if (!app.AadhaarVerified) return RedirectToAction(nameof(Aadhaar));
        if (!app.MobileVerified) return RedirectToAction(nameof(Mobile));
        return View(Build(app, JourneySteps.Pan));
    }

    [HttpPost("pan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pan(PanInput input)
    {
        var app = _state.GetOrCreate();
        input.PanNumber = (input.PanNumber ?? string.Empty).Trim().ToUpperInvariant();
        ModelState.Clear();
        TryValidateModel(input);

        var vm = Build(app, JourneySteps.Pan);
        vm.Pan = input;

        if (!ModelState.IsValid)
            return View(vm);

        var result = await _pan.VerifyPanAsync(input.PanNumber);
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.CustomerMessage ?? "PAN verification failed. Please check your PAN and try again.");
            return View(vm);
        }

        app.PanVerified = true;
        app.PanNumber = input.PanNumber;
        app.PanMasked = result.Data.PanMasked;
        app.JourneyStep = JourneySteps.Vehicle;
        _state.Save(app);
        _audit.PanVerified(app.ApplicationId, result.Data.PanMasked);
        return RedirectToAction(nameof(Vehicle));
    }

    // --------------------------------------------------------------------
    // 6. Vehicle & loan details
    // --------------------------------------------------------------------
    [HttpGet("vehicle")]
    public IActionResult Vehicle()
    {
        var app = _state.GetOrCreate();
        if (!app.PanVerified) return RedirectToAction(nameof(Pan));

        var vm = Build(app, JourneySteps.Vehicle);
        vm.Vehicle = new VehicleInput
        {
            VehicleType = string.IsNullOrEmpty(app.VehicleType) ? VehicleTypes.New : app.VehicleType,
            Manufacturer = app.Manufacturer,
            Model = app.Model,
            Variant = app.Variant,
            RegistrationYear = app.RegistrationYear,
            VehicleCost = app.VehicleCost,
            RequiredLoanAmount = app.RequiredLoanAmount
        };
        return View(vm);
    }

    [HttpPost("vehicle")]
    [ValidateAntiForgeryToken]
    public IActionResult Vehicle(VehicleInput input)
    {
        var app = _state.GetOrCreate();
        if (!ModelState.IsValid)
        {
            var vm = Build(app, JourneySteps.Vehicle);
            vm.Vehicle = input;
            return View(vm);
        }

        app.VehicleType = input.VehicleType;
        app.Manufacturer = input.Manufacturer;
        app.Model = input.Model;
        app.Variant = input.Variant;
        app.RegistrationYear = input.VehicleType == VehicleTypes.Used ? input.RegistrationYear : null;
        app.VehicleCost = input.VehicleCost;
        app.RequiredLoanAmount = input.RequiredLoanAmount;
        app.JourneyStep = JourneySteps.Review;
        _state.Save(app);
        return RedirectToAction(nameof(Review));
    }

    // --------------------------------------------------------------------
    // 7. Review
    // --------------------------------------------------------------------
    [HttpGet("review")]
    public IActionResult Review()
    {
        var app = _state.GetOrCreate();
        if (app.VehicleCost <= 0) return RedirectToAction(nameof(Vehicle));
        return View(Build(app, JourneySteps.Review));
    }

    [HttpPost("review")]
    [ValidateAntiForgeryToken]
    public IActionResult ReviewConfirm(string? mockScenario)
    {
        var app = _state.GetOrCreate();
        if (_cfg.MockMode && !string.IsNullOrEmpty(mockScenario) && MockScenarios.All.Contains(mockScenario))
            app.MockScenario = mockScenario;

        // reset downstream state so re-runs are clean
        app.BureauChecked = false;
        app.BreEvaluated = false;
        app.JourneyStep = JourneySteps.Bureau;
        _state.Save(app);
        return RedirectToAction(nameof(Bureau));
    }

    // --------------------------------------------------------------------
    // 8. Bureau check (processing screen -> auto POST)
    // --------------------------------------------------------------------
    [HttpGet("bureau")]
    public IActionResult Bureau()
    {
        var app = _state.GetOrCreate();
        if (app.VehicleCost <= 0) return RedirectToAction(nameof(Vehicle));
        return View(Build(app, JourneySteps.Bureau));
    }

    [HttpPost("bureau")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunBureau()
    {
        var app = _state.GetOrCreate();
        var result = await _bureau.RunBureauCheckAsync(app);
        if (!result.Success || result.Data is null)
        {
            TempData["ProcessingError"] = result.CustomerMessage;
            return RedirectToAction(nameof(Bureau));
        }

        app.BureauChecked = true;
        app.BureauScore = result.Data.BureauScore;
        app.BureauReportReference = result.Data.ReportReference;
        app.JourneyStep = JourneySteps.Eligibility;
        _state.Save(app);
        _audit.BureauChecked(app.ApplicationId, result.Data.ReportReference);
        return RedirectToAction(nameof(Eligibility));
    }

    // --------------------------------------------------------------------
    // 9. BRE eligibility (processing screen -> auto POST -> result)
    // --------------------------------------------------------------------
    [HttpGet("eligibility")]
    public IActionResult Eligibility()
    {
        var app = _state.GetOrCreate();
        if (!app.BureauChecked) return RedirectToAction(nameof(Bureau));

        // Once evaluated, show the result page.
        if (app.BreEvaluated) return View("Result", Build(app, JourneySteps.Eligibility));

        return View(Build(app, JourneySteps.Eligibility));
    }

    [HttpPost("eligibility")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunBre()
    {
        var app = _state.GetOrCreate();
        if (!app.BureauChecked) return RedirectToAction(nameof(Bureau));

        var result = await _bre.EvaluateAsync(app);
        if (!result.Success || result.Data is null)
        {
            TempData["ProcessingError"] = result.CustomerMessage;
            return RedirectToAction(nameof(Eligibility));
        }

        app.BreEvaluated = true;
        app.Decision = result.Data.Decision;
        app.EligibleAmount = result.Data.EligibleAmount;
        app.DeclineReason = result.Data.DeclineReason;
        app.ReferStatus = result.Data.ReferStatus;
        app.JourneyStep = JourneySteps.Decision;
        _state.Save(app);
        _audit.BreEvaluated(app.ApplicationId, app.Decision);
        _audit.EligibilityDisplayed(app.ApplicationId, app.Decision);
        return RedirectToAction(nameof(Eligibility));
    }

    // --------------------------------------------------------------------
    // 10. Customer decision
    // --------------------------------------------------------------------
    [HttpPost("decision")]
    [ValidateAntiForgeryToken]
    public IActionResult Decision(string customerDecision)
    {
        var app = _state.GetOrCreate();
        if (!app.BreEvaluated) return RedirectToAction(nameof(Eligibility));

        if (customerDecision == CustomerDecisions.NotInterested)
        {
            app.CustomerDecision = CustomerDecisions.NotInterested;
            app.RejectionReason = "Rejected by Customer";
            _state.Save(app);
            _audit.CustomerDecisionCaptured(app.ApplicationId, CustomerDecisions.NotInterested);
            return RedirectToAction(nameof(Closed));
        }

        // Only eligible customers can proceed to lead creation.
        if (app.Decision != Decisions.Eligible)
            return RedirectToAction(nameof(Eligibility));

        app.CustomerDecision = CustomerDecisions.Proceed;
        app.JourneyStep = JourneySteps.EmployeeAssistance;
        _state.Save(app);
        _audit.CustomerDecisionCaptured(app.ApplicationId, CustomerDecisions.Proceed);
        return RedirectToAction(nameof(EmployeeAssistance));
    }

    [HttpPost("refer-submit")]
    [ValidateAntiForgeryToken]
    public IActionResult ReferSubmit()
    {
        var app = _state.GetOrCreate();
        if (!app.BreEvaluated || app.Decision != Decisions.ReferToCredit)
            return RedirectToAction(nameof(Eligibility));

        // Refer-to-credit leads bypass employee assistance and route to the ASM / credit team.
        app.CustomerDecision = CustomerDecisions.Proceed;
        app.AssistedByBankEmployee = "No";
        app.AllocationType = AllocationTypes.Asm;
        app.JourneyStep = JourneySteps.Submitted;
        _state.Save(app);
        _audit.CustomerDecisionCaptured(app.ApplicationId, CustomerDecisions.Proceed);
        return RedirectToAction(nameof(Submit));
    }

    [HttpGet("closed")]
    public IActionResult Closed()
    {
        var app = _state.GetOrCreate();
        return View(Build(app, JourneySteps.Submitted));
    }

    // --------------------------------------------------------------------
    // 11. Bank employee assistance / allocation
    // --------------------------------------------------------------------
    [HttpGet("employee-assistance")]
    public IActionResult EmployeeAssistance()
    {
        var app = _state.GetOrCreate();
        if (app.CustomerDecision != CustomerDecisions.Proceed) return RedirectToAction(nameof(Eligibility));
        return View(Build(app, JourneySteps.EmployeeAssistance));
    }

    [HttpPost("employee-assistance")]
    [ValidateAntiForgeryToken]
    public IActionResult EmployeeAssistance(string assisted)
    {
        var app = _state.GetOrCreate();
        if (app.CustomerDecision != CustomerDecisions.Proceed) return RedirectToAction(nameof(Eligibility));

        if (assisted == "No")
        {
            app.AssistedByBankEmployee = "No";
            app.AllocationType = AllocationTypes.Asm;
            app.BankEmployeeId = string.Empty;
            app.BankEmployeeName = string.Empty;
            _state.Save(app);
            return RedirectToAction(nameof(Submit));
        }

        app.AssistedByBankEmployee = "Yes";
        app.AllocationType = string.Empty;
        _state.Save(app);
        return RedirectToAction(nameof(EmployeeAssistance));
    }

    [HttpPost("employee-assistance/verify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmployee(EmployeeIdInput input)
    {
        var app = _state.GetOrCreate();
        var vm = Build(app, JourneySteps.EmployeeAssistance);
        vm.Employee = input;

        if (!ModelState.IsValid)
            return View(nameof(EmployeeAssistance), vm);

        var result = await _employee.ValidateEmployeeAsync(input.EmployeeId);
        if (!result.Success || result.Data is null)
        {
            // FR-053 / BR-014: do not allocate on failed validation.
            ModelState.AddModelError(string.Empty, result.CustomerMessage ?? "Employee ID could not be verified.");
            return View(nameof(EmployeeAssistance), vm);
        }

        app.BankEmployeeId = result.Data.EmployeeId;
        app.BankEmployeeName = result.Data.EmployeeName;
        app.AllocationType = AllocationTypes.Employee;
        _state.Save(app);
        return RedirectToAction(nameof(Submit));
    }

    // --------------------------------------------------------------------
    // 12. LOS lead creation (processing screen -> auto POST)
    // --------------------------------------------------------------------
    [HttpGet("submit")]
    public IActionResult Submit()
    {
        var app = _state.GetOrCreate();
        if (app.CustomerDecision != CustomerDecisions.Proceed) return RedirectToAction(nameof(Eligibility));
        if (string.IsNullOrEmpty(app.AllocationType)) return RedirectToAction(nameof(EmployeeAssistance));
        if (!string.IsNullOrEmpty(app.LosLeadId)) return RedirectToAction(nameof(Submitted));
        return View(Build(app, JourneySteps.Submitted));
    }

    [HttpPost("submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunLos()
    {
        var app = _state.GetOrCreate();
        if (string.IsNullOrEmpty(app.AllocationType)) return RedirectToAction(nameof(EmployeeAssistance));

        var result = await _los.CreateLeadAsync(app);
        if (!result.Success || result.Data is null)
        {
            TempData["ProcessingError"] = result.CustomerMessage;
            return RedirectToAction(nameof(Submit));
        }

        app.LosLeadId = result.Data.LeadId;
        app.LeadTray = result.Data.Tray.ToString();
        app.JourneyStep = JourneySteps.Submitted;
        _state.Save(app);
        _audit.LeadCreated(app.ApplicationId, app.LosLeadId);
        return RedirectToAction(nameof(Submitted));
    }

    [HttpGet("submitted")]
    public IActionResult Submitted()
    {
        var app = _state.GetOrCreate();
        if (string.IsNullOrEmpty(app.LosLeadId)) return RedirectToAction(nameof(Start));
        return View(Build(app, JourneySteps.Submitted));
    }

    // --------------------------------------------------------------------
    // Reset (kiosk auto-reset / start over)
    // --------------------------------------------------------------------
    [HttpGet("reset")]
    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _state.Reset();
        return RedirectToAction(nameof(Start));
    }
}
