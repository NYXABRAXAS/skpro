namespace SuryodaySelfKiosk.Models;

public enum ApplicationKind { Draft, Submitted, Closed }

/// <summary>Presentation helpers for the "My Applications" screen.</summary>
public static class ApplicationStatusExtensions
{
    public static ApplicationKind Kind(this CarLoanApplication a)
    {
        if (!string.IsNullOrEmpty(a.LosLeadId)) return ApplicationKind.Submitted;
        if (a.CustomerDecision == CustomerDecisions.NotInterested) return ApplicationKind.Closed;
        return ApplicationKind.Draft;
    }

    /// <summary>Customer-friendly status line.</summary>
    public static string StatusLabel(this CarLoanApplication a) => a.Kind() switch
    {
        ApplicationKind.Submitted when a.ReferStatus => "Submitted — under credit review",
        ApplicationKind.Submitted when a.Decision == Decisions.Eligible => "Submitted — approved in principle",
        ApplicationKind.Submitted => "Submitted",
        ApplicationKind.Closed => "Closed — not interested",
        _ => "In progress"
    };

    /// <summary>Where the customer left off (for a draft).</summary>
    public static string NextStepLabel(this CarLoanApplication a)
    {
        if (!a.LoanProcessingConsent || !a.BureauConsent || !a.DeclarationAccepted) return "Consent & Declarations";
        if (!a.AadhaarVerified) return "Aadhaar verification";
        if (!a.MobileVerified) return "Mobile verification";
        if (!a.PanVerified) return "PAN verification";
        if (a.VehicleCost <= 0) return "Vehicle & loan details";
        if (!a.BureauChecked) return "Review & submit for eligibility";
        if (!a.BreEvaluated) return "Eligibility check";
        return "Your decision";
    }

    /// <summary>The route to send the customer to when they resume a draft.</summary>
    public static string ResumePath(this CarLoanApplication a)
    {
        if (!a.LoanProcessingConsent || !a.BureauConsent || !a.DeclarationAccepted) return "/car-loan/consent";
        if (!a.AadhaarVerified) return "/car-loan/aadhaar";
        if (!a.MobileVerified) return "/car-loan/mobile";
        if (!a.PanVerified) return "/car-loan/pan";
        if (a.VehicleCost <= 0) return "/car-loan/vehicle";
        if (!a.BureauChecked) return "/car-loan/review";
        if (!a.BreEvaluated) return "/car-loan/eligibility";
        return "/car-loan/eligibility"; // result page is served from the eligibility action once BreEvaluated
    }

    public static string VehicleSummary(this CarLoanApplication a)
    {
        var type = a.VehicleType == VehicleTypes.Used ? "Used" : a.VehicleType == VehicleTypes.New ? "New" : null;
        var parts = new[] { type, a.Manufacturer, a.Model }.Where(p => !string.IsNullOrWhiteSpace(p));
        var s = string.Join(" ", parts);
        return string.IsNullOrWhiteSpace(s) ? "Car loan" : s + " car loan";
    }
}
