namespace SuryodaySelfKiosk.Models;

/// <summary>
/// Single application model that carries the customer journey state.
/// Persisted in <see cref="Microsoft.AspNetCore.Http.ISession"/> for the prototype
/// (no database). Replace the session store with a real repository for production.
/// </summary>
public class CarLoanApplication
{
    public string ApplicationId { get; set; } = string.Empty;
    public string JourneyStep { get; set; } = JourneySteps.Start;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    // --- Consent ---
    public bool AadhaarConsent { get; set; }
    public bool BureauConsent { get; set; }
    public DateTimeOffset? ConsentCapturedAtUtc { get; set; }

    // --- Mobile ---
    public string MobileNumber { get; set; } = string.Empty;
    public bool MobileVerified { get; set; }
    public bool OtpSent { get; set; }
    public DateTimeOffset? OtpSentAtUtc { get; set; }
    public int OtpResendCount { get; set; }

    // --- Aadhaar eKYC (values are masked for display, never store full Aadhaar) ---
    public bool AadhaarVerified { get; set; }
    public string AadhaarMasked { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // --- PAN ---
    public bool PanVerified { get; set; }
    public string PanNumber { get; set; } = string.Empty;
    public string PanMasked { get; set; } = string.Empty;

    // --- Vehicle & loan ---
    public string VehicleType { get; set; } = string.Empty; // "New" | "Used"
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Variant { get; set; } = string.Empty;
    public int? RegistrationYear { get; set; }
    public decimal VehicleCost { get; set; }
    public decimal RequiredLoanAmount { get; set; }

    // --- Bureau ---
    public bool BureauChecked { get; set; }
    public int BureauScore { get; set; }
    public string BureauReportReference { get; set; } = string.Empty;

    // --- BRE / eligibility ---
    public bool BreEvaluated { get; set; }
    public decimal EligibleAmount { get; set; }
    public string Decision { get; set; } = string.Empty; // Eligible | ReferToCredit | NotEligible
    public string? DeclineReason { get; set; }
    public bool ReferStatus { get; set; }

    // --- Customer decision ---
    public string CustomerDecision { get; set; } = string.Empty; // Proceed | NotInterested
    public string? RejectionReason { get; set; }

    // --- Bank employee assistance / allocation ---
    public string? AssistedByBankEmployee { get; set; } // "Yes" | "No"
    public string BankEmployeeId { get; set; } = string.Empty;
    public string BankEmployeeName { get; set; } = string.Empty;
    public string AllocationType { get; set; } = string.Empty; // "Employee" | "ASM"

    // --- LOS ---
    public string LosLeadId { get; set; } = string.Empty;
    public string LeadTray { get; set; } = string.Empty;

    // --- Developer helper (only honoured when MockMode = true) ---
    public string MockScenario { get; set; } = MockScenarios.Eligible;
}
