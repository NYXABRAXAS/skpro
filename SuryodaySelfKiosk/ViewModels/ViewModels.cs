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

/// <summary>Session-persisted auth state for the "My Applications" mobile-OTP gate.</summary>
public class MyApplicationsAuth
{
    public string Mobile { get; set; } = string.Empty;
    public bool Verified { get; set; }
    public bool OtpSent { get; set; }
    public DateTimeOffset? OtpSentAtUtc { get; set; }
    public int OtpResendCount { get; set; }
}

public class MyApplicationsViewModel
{
    public bool Authenticated { get; set; }
    public bool OtpSent { get; set; }
    public string MaskedMobile { get; set; } = string.Empty;

    public MobileInput Mobile { get; set; } = new();
    public OtpInput Otp { get; set; } = new();

    public List<CarLoanApplication> Drafts { get; set; } = new();
    public List<CarLoanApplication> Submitted { get; set; } = new();
    public List<CarLoanApplication> Closed { get; set; } = new();

    public bool MockMode { get; set; }
    public string? MockOtp { get; set; }
    public int OtpExpirySeconds { get; set; }
    public int MaxOtpResendAttempts { get; set; }
    public int OtpResendCount { get; set; }
}
