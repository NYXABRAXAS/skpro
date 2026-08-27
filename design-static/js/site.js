// Shared UI behaviour for the Suryoday Self Kiosk journey.
(function () {
    "use strict";

    // --- Radio / choice cards -------------------------------------------------
    document.querySelectorAll("[data-choice-group]").forEach(function (group) {
        var cards = group.querySelectorAll(".choice-card");
        function sync() {
            cards.forEach(function (c) {
                var input = c.querySelector("input");
                c.classList.toggle("selected", !!(input && input.checked));
            });
            group.dispatchEvent(new CustomEvent("choice:change"));
        }
        cards.forEach(function (c) {
            c.addEventListener("click", function () {
                var input = c.querySelector("input");
                if (input) { input.checked = true; sync(); }
            });
            c.addEventListener("keydown", function (e) {
                if (e.key === "Enter" || e.key === " ") { e.preventDefault(); c.click(); }
            });
        });
        sync();
    });

    // --- Enable submit only when all mandatory consents are checked ----------
    var consentForm = document.querySelector("[data-consent-form]");
    if (consentForm) {
        var boxes = consentForm.querySelectorAll("input[type=checkbox][data-mandatory]");
        var submit = consentForm.querySelector("[data-consent-submit]");
        function refresh() {
            var all = Array.prototype.every.call(boxes, function (b) { return b.checked; });
            submit.disabled = !all;
        }
        boxes.forEach(function (b) { b.addEventListener("change", refresh); });
        refresh();
    }

    // --- Uppercase PAN as the user types -----------------------------------
    document.querySelectorAll("[data-uppercase]").forEach(function (el) {
        el.addEventListener("input", function () {
            var pos = el.selectionStart;
            el.value = el.value.toUpperCase();
            el.setSelectionRange(pos, pos);
        });
    });

    // --- Numeric-only inputs ----------------------------------------------
    document.querySelectorAll("[data-numeric]").forEach(function (el) {
        el.addEventListener("input", function () {
            el.value = el.value.replace(/\D+/g, "").slice(0, Number(el.dataset.numeric) || 20);
        });
    });

    // --- Live currency formatting hint -----------------------------------
    function formatINR(n) {
        if (isNaN(n)) return "";
        return "₹ " + Number(n).toLocaleString("en-IN");
    }
    document.querySelectorAll("[data-currency-preview]").forEach(function (el) {
        var target = document.querySelector(el.dataset.currencyPreview);
        function upd() { if (target) target.textContent = el.value ? formatINR(el.value) : ""; }
        el.addEventListener("input", upd); upd();
    });

    // --- Loan vs vehicle cost inline check -------------------------------
    var vc = document.querySelector("#VehicleCost");
    var la = document.querySelector("#RequiredLoanAmount");
    var laMsg = document.querySelector("#loanAmountInlineMsg");
    if (vc && la && laMsg) {
        function check() {
            var over = Number(la.value) > 0 && Number(vc.value) > 0 && Number(la.value) > Number(vc.value);
            laMsg.textContent = over ? "Requested loan amount cannot be greater than vehicle cost." : "";
            la.classList.toggle("is-invalid", over);
        }
        vc.addEventListener("input", check);
        la.addEventListener("input", check);
    }

    // --- Auto-submit processing screens --------------------------------
    var auto = document.querySelector("[data-auto-submit]");
    if (auto) {
        var delay = Number(auto.dataset.autoSubmit) || 3200;
        setTimeout(function () { auto.submit(); }, delay);
    }
})();
