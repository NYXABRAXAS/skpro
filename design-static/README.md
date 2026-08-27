# Suryoday Self Kiosk — Static HTML/CSS Design Kit

Plain **HTML + CSS + a little vanilla JS**. No .NET, no build step, no server required.
Open any `.html` file directly in a browser (or serve the folder with any static web server).

This is the **design/markup reference** for the Car Loan Eligibility Self Kiosk journey.
A developer can lift the CSS, markup, and component classes straight into the real app.

## Contents

```
design-static/
├── index.html                    Kiosk home screen (large touchscreen)
├── landing.html                  Mobile landing page (New / Used choice)
├── consent.html                  Consent & Verification
├── aadhaar.html                  Aadhaar — enter number
├── aadhaar-otp.html              Aadhaar — enter OTP (sent to registered mobile)
├── mobile.html                   Mobile — enter number
├── mobile-otp.html               Mobile — enter OTP
├── pan.html                      PAN verification + eKYC details
├── vehicle.html                  Vehicle & loan details
├── review.html                   Review your details
├── bureau-processing.html        Bureau check — processing screen
├── eligibility-processing.html   BRE eligibility — processing screen
├── result-eligible.html          Result — Eligible
├── result-refer.html             Result — Refer to Credit
├── result-not-eligible.html      Result — Not Eligible
├── employee-assistance.html      Bank employee assistance — YES / NO
├── employee-id.html              Bank employee ID entry
├── submit-processing.html        LOS lead creation — processing screen
├── submitted.html                Application submitted (success)
├── closed.html                   Application closed (Not Interested)
├── error.html                    Generic error state
├── components.html               Style guide — every reusable component
│
├── css/
│   ├── suryoday-theme.css         Brand tokens + all shared components (single source of truth)
│   ├── kiosk.css                  Full-screen kiosk home styles
│   └── site.css                   Small resets / focus styles
├── js/
│   ├── site.js                   Choice cards, consent gating, numeric/uppercase inputs, currency preview
│   └── otp.js                    6-box OTP entry + resend countdown
└── images/
    ├── suryoday-logo.svg          Brand logo (replace with official asset if available)
    └── qr-sample.png             Sample QR (encodes https://your-domain.com/car-loan/start)
```

## How the screens are wired

- The pages link to each other so you can click through the **full journey** for review:
  `index → landing → consent → aadhaar → aadhaar-otp → mobile → mobile-otp → pan →
  vehicle → review → bureau-processing → eligibility-processing → result-eligible →
  employee-assistance → employee-id → submit-processing → submitted`
- Processing screens auto-advance after ~3 s (`<meta http-equiv="refresh">`) and also have a manual **Continue** button.
- Forms are **not functional** — buttons just navigate. All validation, OTP checks, API calls,
  and state management belong in the real application.

## Brand colours

| Token | Value | Use |
|---|---|---|
| `--primary-blue` | `#2E3192` | Headings, primary buttons, stepper, icons |
| `--primary-orange` | `#F58220` | CTA buttons, highlights, active indicators |
| `--brand-plum` | `#3a1d5c` | Kiosk hero background, footer |
| `--light-bg` | `#E7E6F2` | Page background, sub-sections |
| `--cream` | `#F8EBD1` | Info panels / business messages |
| `--white` | `#FFFFFF` | Cards, forms |
| `--dark` | `#222222` | Body text |

All tokens live at the top of `css/suryoday-theme.css`.

## Reusable component classes

`card-surface`, `page-title`, `page-subtitle`, `info-panel`,
`btn` + `btn-cta` / `btn-primary` / `btn-outline` / `btn-ghost`, `btn-row` (`.inline`),
`field` + `input` + `hint` + `text-danger`, `alert` (`alert-success` / `alert-danger` / `alert-warning`),
`choice-grid` + `choice-card`, `consent-item`, `otp-inputs`, `stepper`,
`review-group`, `processing` + `spinner` + `check-list`,
`result-icon` (`ok` / `refer` / `no`), `amount-cards` + `amount-card` (`.hi`),
`readonly-block`, `back-link`, `mock-banner`.

See **components.html** for a live example of each.

## Notes for integration

- Replace `images/suryoday-logo.svg` with the official Suryoday logo (keep the filename or update the `<img src>`).
- Regenerate `images/qr-sample.png` — or generate the QR at runtime — from the real journey URL.
- The layout is mobile-first and responsive; the kiosk screen (`index.html`) is tuned for a large touchscreen.
- Minimum touch target is 44px; focus states are visible; colour is never the only status cue.
