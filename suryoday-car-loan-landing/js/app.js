/* ============================================================================
   Suryoday Car Loan landing – wires the configured journey URL into the
   QR code and the New / Used buttons. Pure client-side, no network calls.
   ========================================================================== */
(function () {
  'use strict';

  var url = (document.body.dataset.journeyUrl || '').trim() ||
            'https://your-portal.example.com/car-loan/consent';

  // Show the URL under the QR
  var urlEl = document.getElementById('skQrUrl');
  if (urlEl) urlEl.textContent = url;

  // Point the New / Used buttons at the journey (add ?type= so the portal can pre-select)
  document.querySelectorAll('.sk-option[data-loan-type]').forEach(function (a) {
    var sep = url.indexOf('?') === -1 ? '?' : '&';
    a.setAttribute('href', url + sep + 'type=' + encodeURIComponent(a.dataset.loanType));
  });

  // Render the QR code (vendored qrcode-generator library)
  var target = document.getElementById('skQr');
  if (target && typeof qrcode === 'function') {
    try {
      var qr = qrcode(0, 'M');          // auto version, medium error correction
      qr.addData(url);
      qr.make();
      target.innerHTML = qr.createSvgTag({ cellSize: 4, margin: 4, scalable: true });
    } catch (e) {
      target.textContent = 'QR could not be generated for this URL.';
    }
  }
})();
