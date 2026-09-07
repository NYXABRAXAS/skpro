# Suryoday — Customer Profile view

Standalone **HTML + CSS + vanilla JS**. No framework, no build, works **offline**.
The "Profile" screen a customer sees when they open the profile option in the portal /
mobile app.

```
suryoday-profile/
├── index.html
├── css/style.css     :root config block
├── js/app.js          fills the card from data supplied by the host
└── assets/suryoday-logo.png
```

Shows: **Name**, **Aadhaar Ref No.**, **CIF No.**, **PAN No.** (with a photo / silhouette avatar).

## Use it

Open `index.html` directly, or serve the folder. Two ways to feed real data:

### A. Edit `index.html`
Replace the placeholder values in the `<dd id="…">` elements and the `<h1 id="pfName">`.

### B. Set `window.SURYODAY_PROFILE` (recommended for a portal)
Before `js/app.js` loads:

```html
<script>
  window.SURYODAY_PROFILE = {
    name:         "Rohan Ashok Pawar",
    aadhaarRefNo: "1234 5678 9123",
    cifNo:        "100 200 300 400",
    panNo:        "ABCDE1234F",
    avatarUrl:    null,        // optional customer photo URL
    view:         "auto",      // optional – see below
    showLogo:     false        // optional – Suryoday logo in the orange banner
  };
</script>
<script src="js/app.js"></script>
```

Any value left out keeps the markup's default.

## Web view / Mobile view — configurable

Set `data-view` on `<body>` (or `view` in `SURYODAY_PROFILE`):

| `data-view` | Behaviour |
|---|---|
| `auto` *(default)* | Responsive – a centred card on desktop, a full-screen card on phones (≤ 480px). |
| `mobile` | Always a full-bleed, top-aligned card that fills the viewport – for embedding in a mobile app / webview. |
| `web` | A wider card, vertically centred, roomier spacing – desktop portal "Profile" screen. |

```html
<body class="pf-body" data-view="mobile">   <!-- or "web" / "auto" -->
```

## Customising

All in the `:root` block of `css/style.css`:

| Variable | Purpose |
|---|---|
| `--pf-banner` / `--pf-banner-height` | Orange header colour & height |
| `--pf-card-width` / `--pf-card-width-web` | Card width for mobile vs web view |
| `--pf-blue` | Icon / accent colour |
| `--pf-name` / `--pf-label` / `--pf-value` | Text colours |
| `--pf-radius` | Card corner radius |

- **Show the Suryoday logo** in the banner: add `data-logo` to `<body>` (it renders white on the orange).
- **Avatar**: pass `avatarUrl`; if it fails to load, the built-in SVG silhouette shows.
- **Add / remove a field**: copy a `.pf-row` block in `index.html` (icon + `<dt>` + `<dd id>`), and add the matching `setText(...)` line in `js/app.js` if you drive it from `SURYODAY_PROFILE`.
- To **mask** a value (e.g. show `XXXX XXXX 9123`), format it before assigning — the view renders whatever string it's given.

## Notes

- **100% offline** — no CDN, no web fonts, no network calls. Uses the OS system font
  stack, icons are inline SVG, the avatar silhouette is inline SVG. Open it on a machine
  with no internet and it renders identically.
- Semantic markup (`<dl>` / `<dt>` / `<dd>`), sufficient contrast, scales with the browser font size.
