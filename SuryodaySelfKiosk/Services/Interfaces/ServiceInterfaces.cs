using SuryodaySelfKiosk.Models;

namespace SuryodaySelfKiosk.Services.Interfaces;

// These interfaces are the integration seams. Swap the Mock* implementations
// registered in Program.cs for real API adapters without touching the UI.

public interface IOtpService
{
    Task<ApiResponse<bool>> SendOtpAsync(string mobileNumber);
    Task<ApiResponse<bool>> VerifyOtpAsync(string mobileNumber, string otp);
}

public interface IAadhaarService
{
    /// <summary>Step 1 – triggers UIDAI to send an OTP to the Aadhaar-registered mobile number.</summary>
    Task<ApiResponse<bool>> SendAadhaarOtpAsync(string aadhaarNumber);

    /// <summary>Step 2 – validates the Aadhaar OTP and returns the eKYC profile.</summary>
    Task<ApiResponse<AadhaarKycResult>> VerifyAadhaarOtpAsync(string aadhaarLast4, string otp);
}

public interface IPanService
{
    Task<ApiResponse<PanVerificationResult>> VerifyPanAsync(string pan);
}

public interface IBureauService
{
    Task<ApiResponse<BureauResult>> RunBureauCheckAsync(CarLoanApplication application);
}

public interface IBreService
{
    Task<ApiResponse<BreResult>> EvaluateAsync(CarLoanApplication application);
}

public interface IBankEmployeeService
{
    Task<ApiResponse<BankEmployeeResult>> ValidateEmployeeAsync(string employeeId);
}

public interface ILosService
{
    Task<ApiResponse<LosResult>> CreateLeadAsync(CarLoanApplication application);
}

/// <summary>Audit trail seam. The prototype logs non-sensitive events only.</summary>
public interface IAuditService
{
    void QrScanned(string applicationId);
    void ConsentCaptured(string applicationId, bool aadhaar, bool bureau);
    void OtpVerified(string applicationId, string maskedMobile);
    void AadhaarVerified(string applicationId);
    void PanVerified(string applicationId, string maskedPan);
    void BureauChecked(string applicationId, string reportReference);
    void BreEvaluated(string applicationId, string decision);
    void EligibilityDisplayed(string applicationId, string decision);
    void CustomerDecisionCaptured(string applicationId, string decision);
    void LeadCreated(string applicationId, string losLeadId);
}
