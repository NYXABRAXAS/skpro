namespace SuryodaySelfKiosk.Models;

public enum ApplicationKind { Draft, Submitted, Closed }

/// <summary>Presentation + resume helpers for the "My Applications" screen.</summary>
public static class ApplicationStatusExtensions
{
    public static ApplicationKind Kind(this CarLoanApplication a)
    {
        if (!string.IsNullOrEmpty(a.LosLeadId)) return ApplicationKind.Submitted;
        if (a.CustomerDecision == CustomerDecisions.NotInterested) return ApplicationKind.Closed;
        if (a.BreEvaluated && a.Decision == Decisions.NotEligible) return ApplicationKind.Closed;
        return ApplicationKind.Draft;
    }

    /// <summary>Customer-friendly status line.</summary>
    public static string StatusLabel(this CarLoanApplication a) => a switch
    {
        { LosLeadId.Length: > 0, ReferStatus: true } => "Submitted — under credit review",
        { LosLeadId.Length: > 0 } when a.Decision == Decisions.Eligible => "Submitted — approved in principle",
        { LosLeadId.Length: > 0 } => "Submitted",
        { CustomerDecision: CustomerDecisions.NotInterested } => "Closed — not interested",
        { BreEvaluated: true } when a.Decision == Decisions.NotEligible => "Not eligible at this time",
        _ => "In progress"
    };

    /// <summary>Where the customer left off (label for a draft).</summary>
    public static string NextStepLabel(this CarLoanApplication a)
    {
        if (!a.LoanProcessingConsent || !a.BureauConsent || !a.DeclarationAccepted) return "Consent & Declarations";
        if (!a.AadhaarVerified) return "Aadhaar verification";
        if (!a.MobileVerified) return "Mobile verification";
        if (!a.PanVerified) return "PAN verification";
        if (a.VehicleCost <= 0) return "Vehicle & loan details";
        if (!a.BureauChecked) return "Review & submit for eligibility";
        if (!a.BreEvaluated) return "Eligibility check";
        if (a.CustomerDecision != CustomerDecisions.Proceed) return "Your decision — proceed or not";
        if (string.IsNullOrEmpty(a.AllocationType)) return "Bank employee assistance";
        return "Final submission";
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
        // BRE done – result page shows Proceed / Not Interested
        if (a.CustomerDecision != CustomerDecisions.Proceed) return "/car-loan/eligibility";
        if (string.IsNullOrEmpty(a.AllocationType)) return "/car-loan/employee-assistance";
        return "/car-loan/submit";
    }

    public static string VehicleSummary(this CarLoanApplication a)
    {
        var type = a.VehicleType == VehicleTypes.Used ? "Used" : a.VehicleType == VehicleTypes.New ? "New" : null;
        var parts = new[] { type, a.Manufacturer, a.Model }.Where(p => !string.IsNullOrWhiteSpace(p));
        var s = string.Join(" ", parts);
        return string.IsNullOrWhiteSpace(s) ? "Car loan application" : s + " car loan";
    }
}
