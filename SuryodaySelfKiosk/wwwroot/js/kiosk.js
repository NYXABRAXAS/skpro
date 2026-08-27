// Kiosk inactivity auto-reset. Active only on pages that opt in via
// <body data-session-timeout="120" data-reset-url="/car-loan/reset">.
(function () {
    "use strict";
    var body = document.body;
    var timeout = Number(body.dataset.sessionTimeout || 0);
    var resetUrl = body.dataset.resetUrl;
    if (!timeout || !resetUrl) return;

    var warnAt = Math.max(timeout - 15, 5);
    var elapsed = 0;
    var modal = document.querySelector("#sessionModal");
    var countdownEl = document.querySelector("#sessionCountdown");

    function reset() { elapsed = 0; hideModal(); }
    function hideModal() { if (modal) { modal.classList.remove("show"); modal.setAttribute("aria-hidden", "true"); } }
    function showModal() { if (modal) { modal.classList.add("show"); modal.setAttribute("aria-hidden", "false"); } }

    ["click", "keydown", "touchstart", "mousemove", "input"].forEach(function (ev) {
        document.addEventListener(ev, function () {
            // Don't reset while the customer is actively typing – handled by 'input' firing reset anyway.
            reset();
        }, { passive: true });
    });

    setInterval(function () {
        // Never time out mid-typing.
        var active = document.activeElement;
        if (active && (active.tagName === "INPUT" || active.tagName === "SELECT" || active.tagName === "TEXTAREA") && active.value) {
            elapsed = Math.min(elapsed, warnAt - 1);
        }
        elapsed++;
        if (elapsed >= timeout) { window.location.href = resetUrl; return; }
        if (elapsed >= warnAt) {
            showModal();
            if (countdownEl) countdownEl.textContent = String(timeout - elapsed);
        }
    }, 1000);

    var stay = document.querySelector("#sessionStay");
    if (stay) stay.addEventListener("click", reset);
    var over = document.querySelector("#sessionStartOver");
    if (over) over.addEventListener("click", function () { window.location.href = resetUrl; });
})();
