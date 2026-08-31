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
| Customer journey  | `https://localhost:7042/car-loan/consent`  (the QR target) |

`.NET 10 SDK` required. Ports come from `Properties/launchSettings.json`.

## Journey

The kiosk home (`/`) is the dealer/showroom **poster** — it uses the car-loan photo as a
full-screen background (configurable in `wwwroot/css/kiosk.css` `:root`). The customer
scans the QR (or taps New / Used on the touchscreen) and lands **straight on the consent
screen**:

`Kiosk poster → QR → Consent → (accept all) → Aadhaar eKYC → Mobile OTP → PAN →
Vehicle & Loan → Review → Bureau → BRE Eligibility → Result → Customer Decision →
Bank Employee Assistance → LOS Lead Creation → Success`

`/car-loan/start` (New/Used landing) still exists — the kiosk touch buttons use it to
pre-select the vehicle type before Consent; QR customers pick it on the Vehicle step.

### My Applications

`/car-loan/my-applications` — a returning customer verifies their **mobile number** (OTP)
and sees:

- **Drafts** — journeys they started but didn't finish, with the step they stopped at and
  a **Resume** button that drops them back exactly where they left off.
- **Submitted** — with reference no., status (approved in principle / under credit review),
  amounts and allocation.
- **Closed** — applications they exited with "Not Interested".

Plus a **Start a New Application** button. Entry links sit on the kiosk home and the
consent screen. Storage is `InMemoryApplicationRepository` (a singleton, keyed by
`ApplicationId`, filtered by verified mobile) — lost on restart; swap for a real
repository in production. An application is saved to history the moment the customer
verifies their mobile in the journey.

### Mock credentials / data (shown only when `SelfKiosk:MockMode = true`)

| Field            | Value            |
|------------------|------------------|
| Aadhaar number   | any 12 digits    |
| Aadhaar OTP      | `123456`         |
| Mobile number    | any 10 digits (starts 6–9) |
| Mobile OTP       | `123456`         |
| PAN              | any valid format, e.g. `ABCDE1234F` |
| Bank Employee ID | `EMP001`         |

### Consent screen

`/car-loan/consent` is the "Welcome to Suryoday Car Loan" full-screen consent modal
(`Views/CarLoan/Consent.cshtml` + `_ConsentLayout.cshtml` + `wwwroot/css/consent.css`).
Proceed is disabled until the three mandatory consents are accepted
(Loan Processing, Credit Bureau, bottom Declaration); Communication Consent is optional.

**Background image** – swap it from one place, the `:root` block at the top of
`wwwroot/css/consent.css`:

```css
--desktop-bg-image: url('../images/car-loan-bg.jpg');
--mobile-bg-image:  url('../images/car-loan-bg.jpg');
```

Drop replacement art into `wwwroot/images/` and point these two variables at it
(`--*-bg-position` / `--*-bg-size` / `--bg-overlay` are configurable too). The mobile
image auto-applies at ≤767px.

Aadhaar step = enter Aadhaar number → OTP is sent to the Aadhaar-registered mobile → enter that
OTP → eKYC (name / DOB / address) is returned. Only the last 4 digits of the Aadhaar are ever stored.

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
