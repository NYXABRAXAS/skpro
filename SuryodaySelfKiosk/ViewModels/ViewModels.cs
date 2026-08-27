using SuryodaySelfKiosk.Models;

namespace SuryodaySelfKiosk.ViewModels;

public class KioskHomeViewModel
{
    public string JourneyUrl { get; set; } = string.Empty;
    public string QrDataUri { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public bool MockMode { get; set; }
}

/// <summary>Base view model for every journey step – carries progress + mock flags to the layout.</summary>
public class StepViewModel
{
    public CarLoanApplication App { get; set; } = new();
    public string CurrentStep { get; set; } = JourneySteps.Consent;
    public bool MockMode { get; set; }

    // Per-step payloads (only the relevant one is populated).
    public ConsentInput Consent { get; set; } = new();
    public MobileInput Mobile { get; set; } = new();
    public OtpInput Otp { get; set; } = new();
    public AadhaarInput Aadhaar { get; set; } = new();
    public PanInput Pan { get; set; } = new();
    public VehicleInput Vehicle { get; set; } = new();
    public EmployeeIdInput Employee { get; set; } = new();

    // Mock helpers / config surfaced to views.
    public string? MockOtp { get; set; }
    public string? MockEmployeeId { get; set; }
    public int OtpExpirySeconds { get; set; }
    public int MaxOtpResendAttempts { get; set; }
    public int SessionTimeoutSeconds { get; set; }
    public string[] MockScenarios { get; set; } = Models.MockScenarios.All;
}
