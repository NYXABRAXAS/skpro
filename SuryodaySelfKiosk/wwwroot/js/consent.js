// Consent screen — keep "Proceed" disabled until every mandatory consent is ticked.
(function () {
  'use strict';

  var proceed = document.getElementById('syProceed');
  var hint = document.getElementById('syHint');
  if (!proceed) return;

  var required = Array.prototype.slice.call(document.querySelectorAll('.js-consent-required'));

  function allAccepted() {
    return required.every(function (cb) { return cb.checked; });
  }

  function refresh() {
    var ok = allAccepted();
    proceed.disabled = !ok;
    proceed.setAttribute('aria-disabled', String(!ok));
    if (hint) hint.classList.toggle('is-hidden', ok);
  }

  required.forEach(function (cb) { cb.addEventListener('change', refresh); });
  refresh();
})();
