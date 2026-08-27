using Microsoft.Extensions.Options;
using SuryodaySelfKiosk.Configuration;
using SuryodaySelfKiosk.Models;
using SuryodaySelfKiosk.Services.Interfaces;

namespace SuryodaySelfKiosk.Services.Mock;

// All mock services simulate realistic latency and return static / rule-based data.
// Nothing here calls a real API, database, or third party.

public class MockOtpService(IOptions<SelfKioskOptions> options) : IOtpService
{
    private readonly SelfKioskOptions _cfg = options.Value;

    public async Task<ApiResponse<bool>> SendOtpAsync(string mobileNumber)
    {
        await Task.Delay(1000);
        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<bool>> VerifyOtpAsync(string mobileNumber, string otp)
    {
        await Task.Delay(1000);
        return otp == _cfg.MockOtp
            ? ApiResponse<bool>.Ok(true)
            : ApiResponse<bool>.Fail("Invalid OTP. Please try again.", "OTP_INVALID");
    }
}

public class MockAadhaarService : IAadhaarService
{
    public async Task<ApiResponse<AadhaarKycResult>> VerifyAadhaarAsync(string aadhaarNumber)
    {
        await Task.Delay(2000);

        if (string.IsNullOrWhiteSpace(aadhaarNumber) || aadhaarNumber.Length != 12)
        {
            return ApiResponse<AadhaarKycResult>.Fail(
                "Aadhaar authentication failed. Please try again.", "AADHAAR_FAILED");
        }

        var last4 = aadhaarNumber[^4..];
        return ApiResponse<AadhaarKycResult>.Ok(new AadhaarKycResult
        {
            CustomerName = "Rajesh Kumar",
            DateOfBirth = "15/08/1988",
            Address = "Flat 4B, Sunrise Residency, MG Road, Bengaluru, Karnataka - 560001",
            AadhaarMasked = $"XXXX XXXX {last4}"
        });
    }
}

public class MockPanService : IPanService
{
    public async Task<ApiResponse<PanVerificationResult>> VerifyPanAsync(string pan)
    {
        await Task.Delay(1500);

        pan = (pan ?? string.Empty).ToUpperInvariant();
        var valid = System.Text.RegularExpressions.Regex.IsMatch(pan, "^[A-Z]{5}[0-9]{4}[A-Z]$");
        if (!valid)
        {
            return ApiResponse<PanVerificationResult>.Fail(
                "PAN verification failed. Please check your PAN and try again.", "PAN_INVALID");
        }

        return ApiResponse<PanVerificationResult>.Ok(new PanVerificationResult
        {
            PanMasked = $"{pan[..3]}XXXX{pan[^1]}",
            Name = "RAJESH KUMAR"
        });
    }
}

public class MockBankEmployeeService(IOptions<SelfKioskOptions> options) : IBankEmployeeService
{
    private readonly SelfKioskOptions _cfg = options.Value;

    public async Task<ApiResponse<BankEmployeeResult>> ValidateEmployeeAsync(string employeeId)
    {
        await Task.Delay(1000);

        if (!string.Equals(employeeId?.Trim(), _cfg.MockEmployeeId, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<BankEmployeeResult>.Fail(
                "Employee ID could not be verified. Please check the Employee ID and try again.",
                "EMP_INVALID");
        }

        return ApiResponse<BankEmployeeResult>.Ok(new BankEmployeeResult
        {
            EmployeeId = _cfg.MockEmployeeId,
            EmployeeName = "Amit Sharma"
        });
    }
}
