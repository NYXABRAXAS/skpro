using SuryodaySelfKiosk.Configuration;
using Microsoft.Extensions.Options;
using SuryodaySelfKiosk.Models;
using SuryodaySelfKiosk.Services.Interfaces;

namespace SuryodaySelfKiosk.Services.Mock;

public class MockBureauService(IOptions<SelfKioskOptions> options) : IBureauService
{
    private readonly SelfKioskOptions _cfg = options.Value;

    public async Task<ApiResponse<BureauResult>> RunBureauCheckAsync(CarLoanApplication application)
    {
        await Task.Delay(2000);

        if (_cfg.MockMode && application.MockScenario == MockScenarios.BureauFailure)
        {
            return ApiResponse<BureauResult>.Fail(
                "We are temporarily unable to check your credit profile. Please try again.",
                "BUREAU_UNAVAILABLE");
        }

        var score = application.MockScenario switch
        {
            MockScenarios.NotEligible => 610,
            MockScenarios.ReferToCredit => 705,
            _ => 760
        };

        return ApiResponse<BureauResult>.Ok(new BureauResult
        {
            BureauScore = score,
            ReportReference = $"BUREAU-MOCK-{DateTime.Now:yyyyMMddHHmmss}"
        });
    }
}

public class MockBreService(IOptions<SelfKioskOptions> options) : IBreService
{
    private readonly SelfKioskOptions _cfg = options.Value;

    public async Task<ApiResponse<BreResult>> EvaluateAsync(CarLoanApplication application)
    {
        await Task.Delay(2000);

        if (_cfg.MockMode && application.MockScenario == MockScenarios.BreFailure)
        {
            return ApiResponse<BreResult>.Fail(
                "Unable to evaluate eligibility at this time. Please try again later.",
                "BRE_UNAVAILABLE");
        }

        var requested = application.RequiredLoanAmount;

        // Same policy for New and Used vehicles (BR-007 / FR-032).
        var scenario = _cfg.MockMode
            ? application.MockScenario
            : DeriveScenarioFromScore(application.BureauScore);

        return scenario switch
        {
            MockScenarios.NotEligible => ApiResponse<BreResult>.Ok(new BreResult
            {
                Decision = Decisions.NotEligible,
                RequestedAmount = requested,
                EligibleAmount = 0,
                DeclineReason = "Credit profile does not meet the current eligibility criteria.",
                ReferStatus = false
            }),
            MockScenarios.ReferToCredit => ApiResponse<BreResult>.Ok(new BreResult
            {
                Decision = Decisions.ReferToCredit,
                RequestedAmount = requested,
                EligibleAmount = 0,
                DeclineReason = null,
                ReferStatus = true
            }),
            _ => ApiResponse<BreResult>.Ok(new BreResult
            {
                Decision = Decisions.Eligible,
                RequestedAmount = requested,
                // Offer up to 90% of vehicle cost, capped at the requested amount.
                EligibleAmount = Math.Min(requested, Math.Round(application.VehicleCost * 0.90m, 0)),
                DeclineReason = null,
                ReferStatus = false
            })
        };
    }

    private static string DeriveScenarioFromScore(int score) => score switch
    {
        >= 730 => MockScenarios.Eligible,
        >= 680 => MockScenarios.ReferToCredit,
        _ => MockScenarios.NotEligible
    };
}

public class MockLosService(IOptions<SelfKioskOptions> options) : ILosService
{
    private readonly SelfKioskOptions _cfg = options.Value;

    public async Task<ApiResponse<LosResult>> CreateLeadAsync(CarLoanApplication application)
    {
        await Task.Delay(2000);

        if (_cfg.MockMode && application.MockScenario == MockScenarios.LosFailure)
        {
            return ApiResponse<LosResult>.Fail(
                "We could not submit your application. Please try again or contact support.",
                "LOS_UNAVAILABLE");
        }

        var tray = application.ReferStatus ? LeadTray.ReferToCredit : LeadTray.SoftApproved;

        return ApiResponse<LosResult>.Ok(new LosResult
        {
            LeadId = $"SUR-CAR-{DateTime.Now:yyyy}-{Random.Shared.Next(100000, 999999)}",
            Tray = tray
        });
    }
}
