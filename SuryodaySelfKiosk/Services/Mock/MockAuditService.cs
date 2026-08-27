using SuryodaySelfKiosk.Services.Interfaces;

namespace SuryodaySelfKiosk.Services.Mock;

/// <summary>
/// Prototype audit sink. Writes non-sensitive journey events to the logger.
/// Never logs Aadhaar, OTP, full PAN or raw API responses.
/// </summary>
public class MockAuditService(ILogger<MockAuditService> logger) : IAuditService
{
    private void Write(string ev, string applicationId, string? detail = null) =>
        logger.LogInformation("AUDIT {Event} app={ApplicationId} {Detail}", ev, applicationId, detail ?? "");

    public void QrScanned(string applicationId) => Write("QrScanned", applicationId);

    public void ConsentCaptured(string applicationId, bool aadhaar, bool bureau) =>
        Write("ConsentCaptured", applicationId, $"aadhaar={aadhaar} bureau={bureau}");

    public void OtpVerified(string applicationId, string maskedMobile) =>
        Write("OtpVerified", applicationId, maskedMobile);

    public void AadhaarVerified(string applicationId) => Write("AadhaarVerified", applicationId);

    public void PanVerified(string applicationId, string maskedPan) =>
        Write("PanVerified", applicationId, maskedPan);

    public void BureauChecked(string applicationId, string reportReference) =>
        Write("BureauChecked", applicationId, reportReference);

    public void BreEvaluated(string applicationId, string decision) =>
        Write("BreEvaluated", applicationId, decision);

    public void EligibilityDisplayed(string applicationId, string decision) =>
        Write("EligibilityDisplayed", applicationId, decision);

    public void CustomerDecisionCaptured(string applicationId, string decision) =>
        Write("CustomerDecisionCaptured", applicationId, decision);

    public void LeadCreated(string applicationId, string losLeadId) =>
        Write("LeadCreated", applicationId, losLeadId);
}
