// OTP entry: 6 single-digit boxes bound to one hidden field + resend countdown.
(function () {
    "use strict";
    var wrap = document.querySelector("[data-otp]");
    if (!wrap) return;

    var hidden = document.querySelector("#Otp");
    var boxes = wrap.querySelectorAll("input[data-otp-box]");

    function collect() {
        var v = "";
        boxes.forEach(function (b) { v += (b.value || "").replace(/\D/g, ""); });
        if (hidden) hidden.value = v;
    }

    boxes.forEach(function (box, i) {
        box.addEventListener("input", function () {
            box.value = box.value.replace(/\D/g, "").slice(-1);
            if (box.value && boxes[i + 1]) boxes[i + 1].focus();
            collect();
        });
        box.addEventListener("keydown", function (e) {
            if (e.key === "Backspace" && !box.value && boxes[i - 1]) boxes[i - 1].focus();
        });
        box.addEventListener("paste", function (e) {
            e.preventDefault();
            var digits = (e.clipboardData.getData("text") || "").replace(/\D/g, "").split("");
            boxes.forEach(function (b, k) { b.value = digits[k] || ""; });
            collect();
            (boxes[digits.length] || boxes[boxes.length - 1]).focus();
        });
    });
    if (boxes[0]) boxes[0].focus();

    // Resend countdown
    var btn = document.querySelector("[data-resend-btn]");
    var label = document.querySelector("[data-resend-label]");
    if (btn && label) {
        var seconds = Number(btn.dataset.resendSeconds) || 30;
        var left = seconds;
        btn.disabled = true;
        var t = setInterval(function () {
            left--;
            if (left <= 0) {
                clearInterval(t);
                btn.disabled = false;
                label.textContent = "Didn't get the code?";
            } else {
                label.textContent = "Resend OTP in " + left + " second" + (left === 1 ? "" : "s");
            }
        }, 1000);
    }
})();
