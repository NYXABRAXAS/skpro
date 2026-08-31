# Suryoday Car Loan — QR Code Generator

A tiny **offline** HTML tool to turn the bank-provided Car Loan journey URL into a
downloadable QR code for kiosks, dealer posters and standees.

```
suryoday-qr/
├── index.html          the tool
├── css/style.css
├── js/
│   ├── qrcode.min.js   vendored QR library (qrcode-generator, MIT) – no internet needed
│   └── app.js          tool logic
└── assets/suryoday-logo.svg
```

## Use it

1. Open `index.html` in any browser (double-click, or serve the folder).
2. Paste the **Car Loan journey URL provided by the bank** into the URL box, e.g.
   `https://carloan.suryodaybank.com/car-loan/consent`
3. Choose options (defaults are fine for most cases):
   | Option | Meaning |
   |---|---|
   | **Error correction** | `M` (15%) is recommended. Use `Q`/`H` if the code will be printed small or may get dirty/scuffed. |
   | **PNG size** | Pixel size of the downloaded PNG. Use 512–1024 for screens, 2048 for large print. |
   | **Quiet zone** | White border around the code, in modules. Keep **≥ 4** — scanners need it. |
   | **Module colour** | Black scans most reliably. Suryoday blue is available but needs good print contrast. |
4. Click **Generate QR Code**.
5. **Download PNG** (screens / kiosk UI) or **Download SVG** (print — scales to any poster size without blurring).
6. **Test the downloaded file with a real phone camera before sending it to print.**

## Notes for the developer

- 100% client-side — the URL never leaves the browser, no network calls, works air-gapped.
- The QR just encodes the URL you type. Whatever page that URL points to is what the
  customer's phone opens (in this project: the Car Loan consent screen).
- To change the default URL shown on load, edit the `<textarea id="sqUrl">` value in `index.html`.
- File names of downloads: `suryoday-car-loan-qr.png` / `.svg` (change `state.baseName` in `js/app.js`).
- Library: [`qrcode-generator`](https://github.com/kazuhikoarase/qrcode-generator) v1.4.4, MIT licensed, bundled in `js/`.
