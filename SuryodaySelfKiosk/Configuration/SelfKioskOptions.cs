namespace SuryodaySelfKiosk.Configuration;

/// <summary>
/// Strongly-typed configuration bound from the "SelfKiosk" section of appsettings.json.
/// All business / environment values live here so nothing is hardcoded inside views or services.
/// </summary>
public class SelfKioskOptions
{
    public const string SectionName = "SelfKiosk";

    public string BaseUrl { get; set; } = "https://localhost:5001";
    public string LoanJourneyPath { get; set; } = "/car-loan/consent";

    /// <summary>When true the app runs entirely on mock services and shows developer helpers.</summary>
    public bool MockMode { get; set; } = true;

    public int OtpLength { get; set; } = 6;
    public int OtpExpirySeconds { get; set; } = 60;
    public int MaxOtpResendAttempts { get; set; } = 3;
    public int SessionTimeoutSeconds { get; set; } = 120;

    public string MockOtp { get; set; } = "123456";
    public string MockEmployeeId { get; set; } = "EMP001";

    public string BankName { get; set; } = "Suryoday Small Finance Bank";
    public string ProductName { get; set; } = "Car Loan";

    /// <summary>Absolute URL encoded into the kiosk QR code.</summary>
    public string JourneyUrl => $"{BaseUrl.TrimEnd('/')}{LoanJourneyPath}";
}
