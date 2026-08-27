# Suryoday Bank – Self Kiosk / QR Car Loan Eligibility (Frontend Prototype)

Frontend-only ASP.NET Core MVC prototype of the QR-based Car Loan Eligibility journey
(New & Used Car Loan) for Suryoday Small Finance Bank.

**All integrations are mocked.** No real Aadhaar / PAN / Bureau / BRE / OTP / LOS calls are
made. Mock services simulate realistic latency and return static or rule-based data. The code
is structured so real API adapters can replace the mocks without touching the UI.

## Run locally

```bash
cd SuryodaySelfKiosk
dotnet run
```

Then open:

| Screen            | URL                                   |
|-------------------|---------------------------------------|
| Kiosk home        | `https://localhost:7042/`             |
| Mobile journey    | `https://localhost:7042/car-loan/start` |

`.NET 10 SDK` required. Ports come from `Properties/launchSettings.json`.

## Journey

`Kiosk → QR → Start → Consent → Mobile OTP → Aadhaar eKYC → PAN → Vehicle & Loan →
Review → Bureau → BRE Eligibility → Result → Customer Decision → Bank Employee Assistance →
LOS Lead Creation → Success`

### Mock credentials / data (shown only when `SelfKiosk:MockMode = true`)

| Field           | Value            |
|-----------------|------------------|
| OTP             | `123456`         |
| Aadhaar         | any 12 digits    |
| PAN             | any valid format, e.g. `ABCDE1234F` |
| Bank Employee ID| `EMP001`         |

On the **Review** screen (mock mode only) a *scenario* selector drives the simulated outcome:
`Eligible`, `ReferToCredit`, `NotEligible`, `BureauFailure`, `BreFailure`, `LosFailure`.

## Configuration – `appsettings.json` → `SelfKiosk`

| Key                     | Purpose                                             |
|-------------------------|-----------------------------------------------------|
| `BaseUrl` / `LoanJourneyPath` | QR code target (`BaseUrl + LoanJourneyPath`)   |
| `MockMode`              | Enables mock services + developer helpers           |
| `OtpLength`, `OtpExpirySeconds`, `MaxOtpResendAttempts` | OTP behaviour            |
| `SessionTimeoutSeconds` | Kiosk inactivity auto-reset                          |
| `MockOtp`, `MockEmployeeId` | Demo values                                      |

## Replacing mocks with real APIs

1. Implement the interfaces in `Services/Interfaces/ServiceInterfaces.cs`
   (`IOtpService`, `IAadhaarService`, `IPanService`, `IBureauService`, `IBreService`,
   `IBankEmployeeService`, `ILosService`, `IAuditService`).
2. Register the real implementations in `Program.cs` in place of the `Mock*` classes.
3. No view or controller changes required. `CarLoanApplication` already carries every field
   the LOS payload needs (customer, bureau, eligibility, loan, allocation).

## Structure

```
Configuration/SelfKioskOptions.cs     strongly-typed config
Controllers/KioskController.cs         kiosk home + QR
Controllers/CarLoanController.cs       the whole journey (attribute-routed /car-loan/*)
Models/                                CarLoanApplication, enums, service DTOs, step inputs
Services/Interfaces/                   integration seams
Services/Mock/                         mock implementations (latency + sample data)
Services/QrCodeService.cs              server-side QR (QRCoder, managed PNG)
Services/ApplicationStateService.cs    session-backed prototype store (no DB)
ViewModels/                            KioskHomeViewModel, StepViewModel
Views/Kiosk/ , Views/CarLoan/          screens
Views/Shared/                          _Layout, _KioskLayout, header/footer/stepper partials
wwwroot/css/suryoday-theme.css         brand tokens + shared components
wwwroot/css/kiosk.css                  full-screen kiosk styles
wwwroot/js/site.js, otp.js, kiosk.js   choice cards, OTP boxes, inactivity reset
```

## Notes

- State lives in `HttpContext.Session` (prototype only – swap for a real store).
- Aadhaar and PAN are masked everywhere; full values are never rendered or logged.
- Audit events are logged (non-sensitive only) via `IAuditService`.
- No database, Redis, or Docker required.
