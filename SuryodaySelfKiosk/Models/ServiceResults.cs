namespace SuryodaySelfKiosk.Models;

/// <summary>Generic mock-service envelope so real API adapters can drop in later.</summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public string? CustomerMessage { get; set; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string customerMessage, string? errorCode = null) =>
        new() { Success = false, CustomerMessage = customerMessage, ErrorCode = errorCode };
}

public class AadhaarKycResult
{
    public string CustomerName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AadhaarMasked { get; set; } = string.Empty;
}

public class PanVerificationResult
{
    public string PanMasked { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class BureauResult
{
    public int BureauScore { get; set; }
    public string ReportReference { get; set; } = string.Empty;
}

public class BreResult
{
    public string Decision { get; set; } = Decisions.Eligible;
    public decimal RequestedAmount { get; set; }
    public decimal EligibleAmount { get; set; }
    public string? DeclineReason { get; set; }
    public bool ReferStatus { get; set; }
}

public class BankEmployeeResult
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
}

public class LosResult
{
    public string LeadId { get; set; } = string.Empty;
    public LeadTray Tray { get; set; } = LeadTray.AllLeads;
}
