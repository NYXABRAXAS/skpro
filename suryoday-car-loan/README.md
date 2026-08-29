# Suryoday Car Loan — Consent / Eligibility Landing

Production-ready, responsive **HTML + Bootstrap 5 + vanilla JS** page for the
Suryoday Small Finance Bank car-loan consent screen. No framework, no build step.

```
suryoday-car-loan/
├── index.html            markup only
├── css/
│   └── style.css         all styling + configuration block
├── js/
│   └── script.js         checkbox gating + LOS integration hooks
└── assets/
    ├── Background.jpg.jpeg   full-screen background
    └── suryoday-logo.svg     header logo (swap with the official asset)
```

Bootstrap 5 and Bootstrap Icons load from the jsDelivr CDN (see the `<link>`/`<script>`
tags in `index.html`). To run fully offline, download those files into `assets/`
and repoint the tags.

## Run / preview

Open `index.html` directly in a browser, or serve the folder:

```bash
cd suryoday-car-loan
python -m http.server 8080
```
then open `http://localhost:8080/`.

---

## 1. Replacing the background images (desktop & mobile)

Everything is driven from the `:root` block at the **top of `css/style.css`** — one place, no HTML changes:

```css
:root {
  --desktop-bg-image: url('../assets/Background.jpg.jpeg');
  --mobile-bg-image:  url('../assets/Background.jpg.jpeg');

  --desktop-bg-position: center center;
  --mobile-bg-position:  center top;

  --desktop-bg-size: cover;   /* cover | contain | 100% auto | ... */
  --mobile-bg-size:  cover;

  --bg-overlay: rgba(20, 28, 46, 0.28);  /* dim layer over the image; set alpha 0 to remove */
}
```

Example — separate art for each breakpoint:

```css
--desktop-bg-image: url('../assets/web-background.jpg');
--mobile-bg-image:  url('../assets/mobile-background.jpg');
```

- Desktop/tablet uses `--desktop-bg-*`.
- At **≤ 767.98px** the media query automatically switches to `--mobile-bg-*`.
- The design does **not** depend on the current image's dimensions — any size works.

---

## 2. Integrating Proceed / Cancel into the LOS

All wiring lives at the bottom of `js/script.js` — three stub functions, nothing else to touch:

```js
window.handleProceed = function (consent) {
  // consent = { loanProcessingConsent, creditBureauConsent,
  //             communicationConsent, declarationAccepted, capturedAt }
  // e.g. POST consent to your API, then navigate to the eligibility form:
  // fetch('/api/car-loan/consent', { method:'POST',
  //   headers:{'Content-Type':'application/json'}, body: JSON.stringify(consent) })
  //   .then(() => window.location.href = '/car-loan/start');
};

window.handleCancel = function () {
  // e.g. window.location.href = '/dashboard';
};

window.handlePrivacyPolicy = function () {
  // e.g. window.open('/legal/privacy-policy', '_blank', 'noopener');
};
```

**Gating logic (already implemented):** the **Proceed** button stays `disabled` until
every checkbox with the class **`js-consent-required`** is ticked:

| Checkbox | id | Class | Mandatory |
|---|---|---|---|
| Loan Processing Consent | `consentLoanProcessing` | `js-consent-required` | ✅ |
| Credit Bureau Consent | `consentCreditBureau` | `js-consent-required` | ✅ |
| Bottom Declaration | `declarationAgree` | `js-consent-required` | ✅ |
| Communication Consent | `consentCommunication` | `js-consent-optional` | ❌ (optional) |

To make a checkbox mandatory/optional, just change its class in `index.html`.
The **X** (close) button calls `handleCancel()` — the same as Cancel.

---

## 3. Changing the Suryoday colours

Also in the `:root` block of `css/style.css`:

```css
--sy-blue:        #1b3a8b;   /* primary dark blue – header, Proceed button, headings */
--sy-blue-dark:   #142d6b;   /* hover / pressed */
--sy-orange:      #f5821f;   /* secondary accent – section labels, card icons, check marks */
--sy-orange-dark: #d96c0c;

--sy-info-bg:     #eef4fd;   /* light-blue journey banner / security / declaration panels */
--sy-info-border: #d5e5fb;

--sy-heading:     #16357e;   /* card & banner headings */
--sy-text:        #3d4351;   /* body text */
--sy-legal:       #4b5160;   /* consent / legal paragraphs */

--sy-radius:      18px;      /* modal corner radius */
--sy-radius-sm:   12px;      /* cards / panels / buttons */
```

Change the value once and it propagates everywhere. No other edits needed.
The logo colours live in `assets/suryoday-logo.svg` (`fill="#1b3a8b"` / `fill="#f5821f"`) —
replace the whole file with the official Suryoday logo when available (keep the filename
or update the `<img src>` in `index.html`).

---

## Responsive behaviour

| Width | Layout |
|---|---|
| ≥ 992px | Centered modal (max 1240px), 3 cards side-by-side, header title centered, banner horizontal, modal body scrolls internally so header/footer stay pinned |
| 768–991px | Cards 2-up (`col-md-6`), banner stacks vertically, header left-aligned |
| ≤ 767px | Full-bleed (no radius/shadow), mobile background image, cards stacked, banner + note stacked & centered, footer buttons full-width and stacked, natural page scroll |
| ≤ 360px | Tighter padding; still no horizontal scroll, no clipped text |

Accessibility: semantic landmarks, every checkbox has a `<label for>`, the close button
has `aria-label`, decorative icons are `aria-hidden`, visible focus rings, and the disabled
state is also communicated by the text hint next to the buttons (not colour alone).
