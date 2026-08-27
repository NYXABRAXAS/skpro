namespace SuryodaySelfKiosk.Models;

/// <summary>Ordered journey steps used by the progress stepper and back / forward guards.</summary>
public static class JourneySteps
{
    public const string Start = "Start";
    public const string Consent = "Consent";
    public const string Mobile = "Mobile";
    public const string Aadhaar = "Aadhaar";
    public const string Pan = "Pan";
    public const string Vehicle = "Vehicle";
    public const string Review = "Review";
    public const string Bureau = "Bureau";
    public const string Eligibility = "Eligibility";
    public const string Decision = "Decision";
    public const string EmployeeAssistance = "EmployeeAssistance";
    public const string Submitted = "Submitted";

    /// <summary>The 7 milestones shown to the customer in the progress indicator.</summary>
    public static readonly (string Key, string Label)[] Milestones =
    {
        (Consent, "Consent"),
        (Mobile, "Mobile"),
        (Aadhaar, "Aadhaar"),
        (Pan, "PAN"),
        (Vehicle, "Vehicle"),
        (Eligibility, "Eligibility"),
        (Decision, "Decision"),
    };
}

public static class VehicleTypes
{
    public const string New = "New";
    public const string Used = "Used";
}

public static class Decisions
{
    public const string Eligible = "Eligible";
    public const string ReferToCredit = "ReferToCredit";
    public const string NotEligible = "NotEligible";
}

public static class CustomerDecisions
{
    public const string Proceed = "Proceed";
    public const string NotInterested = "NotInterested";
}

public static class AllocationTypes
{
    public const string Employee = "Employee";
    public const string Asm = "ASM";
}

/// <summary>ASM tray placeholder – where a lead would surface in LOS after integration.</summary>
public enum LeadTray
{
    AllLeads,
    SoftApproved,
    ReferToCredit
}

/// <summary>Developer-only scenarios, honoured only when MockMode = true.</summary>
public static class MockScenarios
{
    public const string Eligible = "Eligible";
    public const string ReferToCredit = "ReferToCredit";
    public const string NotEligible = "NotEligible";
    public const string BureauFailure = "BureauFailure";
    public const string BreFailure = "BreFailure";
    public const string LosFailure = "LosFailure";

    public static readonly string[] All =
    {
        Eligible, ReferToCredit, NotEligible, BureauFailure, BreFailure, LosFailure
    };
}
