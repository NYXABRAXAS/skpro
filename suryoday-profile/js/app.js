/* ============================================================================
   Suryoday Profile view – fills the card from data supplied by the host.
   100% client-side, no network calls.

   The portal can set this BEFORE app.js loads:

     <script>
       window.SURYODAY_PROFILE = {
         name:         "Rohan Ashok Pawar",
         aadhaarRefNo: "1234 5678 9123",
         cifNo:        "100 200 300 400",
         panNo:        "ABCDE1234F",
         avatarUrl:    null,      // optional customer photo URL
         view:         "auto",    // "auto" | "mobile" | "web"  (optional)
         showLogo:     false      // optional – Suryoday logo in the banner
       };
     </script>
     <script src="js/app.js"></script>

   ...or just edit the values in index.html.
   ========================================================================== */
(function () {
  'use strict';

  var data = window.SURYODAY_PROFILE || {};

  function setText(id, value) {
    var el = document.getElementById(id);
    if (el && value != null && String(value).trim() !== '') {
      el.textContent = String(value);
    }
  }

  setText('pfName', data.name);
  setText('pfAadhaarRefNo', data.aadhaarRefNo);
  setText('pfCifNo', data.cifNo);
  setText('pfPanNo', data.panNo);

  // Optional photo – replaces the built-in silhouette; falls back to it on error.
  if (data.avatarUrl) {
    var wrap = document.getElementById('pfAvatar');
    if (wrap) {
      var img = new Image();
      img.alt = '';
      img.onload = function () { wrap.innerHTML = ''; wrap.appendChild(img); };
      img.onerror = function () { /* keep the silhouette */ };
      img.src = data.avatarUrl;
    }
  }

  if (data.view) document.body.setAttribute('data-view', data.view);
  if (data.showLogo) document.body.setAttribute('data-logo', '');
})();
