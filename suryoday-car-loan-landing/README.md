# Suryoday Car Loan — "Check Your Car Loan Eligibility" landing screen

Standalone **HTML + CSS + vanilla JS**. No framework, no build, **works offline**.
Drop the folder into a portal and open `index.html`, or embed the markup.

```
suryoday-car-loan-landing/
├── index.html
├── css/style.css
├── js/
│   ├── qrcode.min.js   vendored QR library (qrcode-generator, MIT)
│   └── app.js          wires the journey URL into the QR + buttons
└── assets/
    ├── car-loan-bg.jpg     background photo
    └── suryoday-logo.svg
```

## 1. Set the journey URL

In `index.html`, edit the one attribute on `<body>`:

```html
<body class="sk-body" data-journey-url="https://your-portal.example.com/car-loan/consent">
```

That URL is used for **both** the QR code and the NEW / USED buttons
(the buttons get `?type=New` / `?type=Used` appended so the portal can pre-select).

## 2. Background image — "show the whole photo"

All in the `:root` block at the top of `css/style.css`:

```css
--sk-bg-image:    url('../assets/car-loan-bg.jpg');
--sk-bg-size:     contain;      /* contain = the ENTIRE image is always visible (default)
                                   cover   = fill the screen and crop the edges */
--sk-bg-position: left center;
--sk-bg-color:    #f4f3f1;      /* fills the area around a 'contain' image – set to match your photo's edges */
```

- **`contain`** (default): the complete image is shown, never cropped. Any spare space
  around it is filled with `--sk-bg-color` (set to near-white so the showroom edges blend).
- **`cover`**: the image fills the whole screen but the top/bottom (or sides) are cropped.

On phones/tablets (≤ 991px) the CSS automatically switches to a cropped, darkened
backdrop with the card centred — a small "contained" image looks lost on a narrow screen.

To swap the photo, replace `assets/car-loan-bg.jpg` (or point `--sk-bg-image` elsewhere)
and adjust `--sk-bg-color` to match its edges.

## 3. Brand colours

Also in `:root`: `--sk-blue`, `--sk-orange`, `--sk-heading`. Change once, applies everywhere.
The logo lives in `assets/suryoday-logo.svg` — replace with the official asset (keep the filename).

## Notes

- 100% offline — no CDN, no network calls. The QR is generated in the browser from the URL you set.
- No "view your applications" link — this is a clean entry screen only.
- Test the QR with a real phone camera before going live.
