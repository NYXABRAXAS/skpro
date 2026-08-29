/* ============================================================================
   Suryoday Small Finance Bank – Car Loan Consent Landing
   script.js  (vanilla JavaScript, no dependencies beyond the DOM)

   INTEGRATION POINTS
   ------------------
   - handleProceed() : called when the user clicks "Proceed" with all
                       mandatory consents accepted. Wire your LOS/API call here.
   - handleCancel()  : called when the user clicks "Cancel" or the close (X)
                       button. Wire your LOS close/back action here.
   ========================================================================== */
(function () {
  'use strict';

  var proceedBtn = document.getElementById('syProceed');
  var cancelBtn  = document.getElementById('syCancel');
  var closeBtn   = document.getElementById('syClose');
  var hint       = document.getElementById('syHint');
  var privacyLnk = document.getElementById('syPrivacyPolicy');

  // Mandatory consents = every checkbox with .js-consent-required
  var requiredBoxes = Array.prototype.slice.call(
    document.querySelectorAll('.js-consent-required')
  );

  /** True only when every mandatory checkbox is ticked. */
  function allMandatoryAccepted() {
    return requiredBoxes.every(function (cb) { return cb.checked; });
  }

  /** Enable/disable Proceed and toggle the helper hint. */
  function refreshState() {
    var ok = allMandatoryAccepted();
    proceedBtn.disabled = !ok;
    proceedBtn.setAttribute('aria-disabled', String(!ok));
    if (hint) hint.classList.toggle('is-hidden', ok);
  }

  requiredBoxes.forEach(function (cb) {
    cb.addEventListener('change', refreshState);
  });

  // --- Proceed -------------------------------------------------------------
  proceedBtn.addEventListener('click', function () {
    if (proceedBtn.disabled || !allMandatoryAccepted()) {
      refreshState();
      return;
    }
    handleProceed(collectConsentState());
  });

  // --- Cancel / Close ----------------------------------------------------
  cancelBtn.addEventListener('click', function () { handleCancel(); });
  if (closeBtn) closeBtn.addEventListener('click', function () { handleCancel(); });

  // --- Privacy Policy link ---------------------------------------------
  if (privacyLnk) {
    privacyLnk.addEventListener('click', function (e) {
      e.preventDefault();
      handlePrivacyPolicy();
    });
  }

  /** Snapshot of all consent selections – handy for the LOS payload. */
  function collectConsentState() {
    return {
      loanProcessingConsent: getChecked('consentLoanProcessing'),
      creditBureauConsent:   getChecked('consentCreditBureau'),
      communicationConsent:  getChecked('consentCommunication'), // optional
      declarationAccepted:   getChecked('declarationAgree'),
      capturedAt:            new Date().toISOString()
    };
  }
  function getChecked(id) {
    var el = document.getElementById(id);
    return !!(el && el.checked);
  }

  // Initial paint
  refreshState();

  /* ========================================================================
     REPLACE THE FUNCTIONS BELOW WITH YOUR LOS INTEGRATION
     ====================================================================== */

  /**
   * Called when the user proceeds with all mandatory consents accepted.
   * @param {Object} consent - see collectConsentState()
   */
  window.handleProceed = function handleProceed(consent) {
    // Example: persist consent, then route to the eligibility form.
    // fetch('/api/car-loan/consent', {
    //   method: 'POST',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify(consent)
    // }).then(function () { window.location.href = '/car-loan/start'; });

    console.log('handleProceed – integrate LOS submission here', consent);
    alert('Consent captured. Integrate LOS submission in handleProceed().');
  };

  /**
   * Called when the user cancels or closes the consent screen.
   */
  window.handleCancel = function handleCancel() {
    // Example: window.location.href = '/dashboard';
    console.log('handleCancel – integrate LOS close/back action here');
  };

  /**
   * Called when the user clicks the "Privacy Policy" link.
   */
  window.handlePrivacyPolicy = function handlePrivacyPolicy() {
    // Example: window.open('/legal/privacy-policy', '_blank', 'noopener');
    console.log('handlePrivacyPolicy – open the Privacy Policy document here');
  };
})();
