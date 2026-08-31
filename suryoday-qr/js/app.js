/* ============================================================================
   Suryoday Car Loan – QR Code Generator
   Pure client-side. Uses the vendored qrcode-generator library (js/qrcode.min.js).
   Nothing is uploaded anywhere – everything runs in the browser.
   ========================================================================== */
(function () {
  'use strict';

  var el = {
    url:        document.getElementById('sqUrl'),
    ecc:        document.getElementById('sqEcc'),
    size:       document.getElementById('sqSize'),
    margin:     document.getElementById('sqMargin'),
    dark:       document.getElementById('sqDark'),
    generate:   document.getElementById('sqGenerate'),
    error:      document.getElementById('sqError'),
    preview:    document.getElementById('sqPreview'),
    canvas:     document.getElementById('sqCanvas'),
    placeholder: document.querySelector('.sq-preview__placeholder'),
    meta:       document.getElementById('sqMeta'),
    metaUrl:    document.getElementById('sqMetaUrl'),
    metaVersion: document.getElementById('sqMetaVersion'),
    downloads:  document.getElementById('sqDownloads'),
    dlPng:      document.getElementById('sqDownloadPng'),
    dlSvg:      document.getElementById('sqDownloadSvg')
  };

  var LIGHT = '#ffffff';
  var state = { svg: '', baseName: 'suryoday-car-loan-qr' };

  function showError(msg) {
    el.error.textContent = msg;
    el.error.hidden = false;
  }
  function clearError() { el.error.hidden = true; el.error.textContent = ''; }

  /** Build the QR model, auto-selecting the smallest version that fits. */
  function buildQr(text, ecc) {
    var qr = qrcode(0, ecc);          // 0 = auto type number
    qr.addData(text);
    qr.make();
    return qr;
  }

  /** Draw the QR onto the canvas with crisp, integer-sized modules. */
  function renderCanvas(qr, targetPx, marginModules, dark) {
    var count = qr.getModuleCount();
    var totalModules = count + marginModules * 2;
    var moduleSize = Math.max(1, Math.floor(targetPx / totalModules));
    var dim = moduleSize * totalModules;

    var canvas = el.canvas;
    canvas.width = dim;
    canvas.height = dim;

    var ctx = canvas.getContext('2d');
    ctx.fillStyle = LIGHT;
    ctx.fillRect(0, 0, dim, dim);
    ctx.fillStyle = dark;
    for (var r = 0; r < count; r++) {
      for (var c = 0; c < count; c++) {
        if (qr.isDark(r, c)) {
          ctx.fillRect(
            (c + marginModules) * moduleSize,
            (r + marginModules) * moduleSize,
            moduleSize, moduleSize
          );
        }
      }
    }
    canvas.style.width = Math.min(dim, 300) + 'px';
    canvas.hidden = false;
    if (el.placeholder) el.placeholder.style.display = 'none';
    return { dim: dim, count: count };
  }

  /** Build a resolution-independent SVG string for print. */
  function buildSvg(qr, marginModules, dark) {
    var count = qr.getModuleCount();
    var total = count + marginModules * 2;
    var path = '';
    for (var r = 0; r < count; r++) {
      for (var c = 0; c < count; c++) {
        if (qr.isDark(r, c)) {
          path += 'M' + (c + marginModules) + ' ' + (r + marginModules) + 'h1v1h-1z';
        }
      }
    }
    return '<?xml version="1.0" encoding="UTF-8"?>\n' +
      '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ' + total + ' ' + total + '" ' +
      'width="1024" height="1024" shape-rendering="crispEdges">' +
      '<rect width="' + total + '" height="' + total + '" fill="' + LIGHT + '"/>' +
      '<path d="' + path + '" fill="' + dark + '"/>' +
      '</svg>';
  }

  function generate() {
    clearError();
    var url = (el.url.value || '').trim();
    if (!url) { showError('Please enter the Car Loan journey URL.'); return; }
    if (!/^https?:\/\//i.test(url)) {
      showError('The URL should start with https:// (or http://). Please check the address from the bank.');
      return;
    }

    var ecc = el.ecc.value;
    var marginModules = parseInt(el.margin.value, 10) || 0;
    var targetPx = parseInt(el.size.value, 10) || 512;
    var dark = el.dark.value || '#000000';

    var qr;
    try {
      qr = buildQr(url, ecc);
    } catch (e) {
      showError('This URL is too long to encode at the selected error-correction level. ' +
        'Try a shorter URL or a lower error-correction level (L or M).');
      return;
    }

    var info = renderCanvas(qr, targetPx, marginModules, dark);
    state.svg = buildSvg(qr, marginModules, dark);

    var version = (info.count - 17) / 4;
    el.metaUrl.textContent = url;
    el.metaVersion.textContent = 'Version ' + version + '  ·  ' + info.count + ' × ' + info.count +
      ' modules  ·  PNG ' + info.dim + ' px';
    el.meta.hidden = false;
    el.downloads.hidden = false;
  }

  function downloadBlob(blob, filename) {
    var a = document.createElement('a');
    var href = URL.createObjectURL(blob);
    a.href = href;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(function () { URL.revokeObjectURL(href); }, 1500);
  }

  function downloadPng() {
    if (el.canvas.hidden) return;
    el.canvas.toBlob(function (blob) {
      downloadBlob(blob, state.baseName + '.png');
    }, 'image/png');
  }

  function downloadSvg() {
    if (!state.svg) return;
    downloadBlob(new Blob([state.svg], { type: 'image/svg+xml' }), state.baseName + '.svg');
  }

  // --- wire up ----------------------------------------------------------
  if (typeof qrcode === 'undefined') {
    showError('QR library failed to load. Make sure js/qrcode.min.js is present next to this page.');
    el.generate.disabled = true;
    return;
  }

  el.generate.addEventListener('click', generate);
  el.dlPng.addEventListener('click', downloadPng);
  el.dlSvg.addEventListener('click', downloadSvg);
  el.url.addEventListener('keydown', function (e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') generate();
  });

  // Generate once on load so the developer sees a working example immediately.
  generate();
})();
