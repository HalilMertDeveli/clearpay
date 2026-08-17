(() => {
  const root = document.querySelector("[data-card-preview]");
  const form = document.querySelector("[data-card-form]");
  if (!root || !form) {
    return;
  }

  const card = document.getElementById("live-card");
  const panEl = root.querySelector("[data-card-pan]");
  const nameEl = root.querySelector("[data-card-name]");
  const expEl = root.querySelector("[data-card-exp]");
  const cvvEl = root.querySelector("[data-card-cvv]");
  const schemeEl = root.querySelector("[data-card-scheme]");
  const numberInput = form.querySelector('[data-card-input="number"]');
  const nameInput = form.querySelector('[data-card-input="name"]');
  const labelInput = form.querySelector('[data-card-input="label"]');
  const expiryInput = form.querySelector('[data-card-input="expiry"]');
  const cvvInput = form.querySelector('[data-card-input="cvv"]');
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  const detectScheme = (digits) => {
    if (digits.startsWith("9792")) {
      return "troy";
    }
    if (digits.charAt(0) === "4") {
      return "visa";
    }
    const two = Number(digits.slice(0, 2));
    const four = Number(digits.slice(0, 4));
    if ((two >= 51 && two <= 55) || (four >= 2221 && four <= 2720)) {
      return "mastercard";
    }
    return "unknown";
  };

  const schemeLabel = (scheme) => {
    if (scheme === "visa") {
      return "Visa";
    }
    if (scheme === "mastercard") {
      return "Mastercard";
    }
    if (scheme === "troy") {
      return "Troy";
    }
    return schemeEl ? schemeEl.textContent : "";
  };

  const groupPan = (digits) => {
    const padded = (digits + "••••••••••••••••").slice(0, 16);
    return padded.replace(/(.{4})/g, "$1 ").trim();
  };

  const paint = () => {
    const digits = (numberInput?.value || "").replace(/\D/g, "").slice(0, 16);
    const scheme = detectScheme(digits);
    if (card) {
      card.setAttribute("data-scheme", scheme);
    }
    if (panEl) {
      panEl.textContent = groupPan(digits);
    }
    if (schemeEl) {
      schemeEl.textContent = schemeLabel(scheme);
    }
    const displayName = (labelInput?.value || nameInput?.value || "").trim();
    if (nameEl) {
      nameEl.textContent = displayName || nameEl.getAttribute("data-placeholder") || nameEl.textContent;
    }
    if (expEl) {
      const raw = (expiryInput?.value || "").replace(/\D/g, "").slice(0, 4);
      expEl.textContent = raw.length === 0 ? "MM/YY" : raw.length <= 2 ? raw : raw.slice(0, 2) + "/" + raw.slice(2);
    }
    if (cvvEl) {
      const cvv = (cvvInput?.value || "").replace(/\D/g, "").slice(0, 4);
      cvvEl.textContent = cvv.length === 0 ? "•••" : cvv.replace(/./g, "•");
    }
  };

  const formatNumber = () => {
    if (!numberInput) {
      return;
    }
    const digits = numberInput.value.replace(/\D/g, "").slice(0, 16);
    numberInput.value = digits.replace(/(.{4})/g, "$1 ").trim();
  };

  const formatExpiry = () => {
    if (!expiryInput) {
      return;
    }
    const digits = expiryInput.value.replace(/\D/g, "").slice(0, 4);
    expiryInput.value = digits.length <= 2 ? digits : digits.slice(0, 2) + "/" + digits.slice(2);
  };

  form.addEventListener("input", (event) => {
    const target = event.target;
    if (!(target instanceof HTMLInputElement)) {
      return;
    }
    if (target.getAttribute("data-card-input") === "number") {
      formatNumber();
    }
    if (target.getAttribute("data-card-input") === "expiry") {
      formatExpiry();
    }
    paint();
  });

  if (cvvInput && !reduce) {
    const flip = (on) => {
      root.classList.toggle("is-flipped", on);
    };
    cvvInput.addEventListener("focus", () => flip(true));
    cvvInput.addEventListener("blur", () => flip(false));
  }

  form.addEventListener("submit", () => {
    if (cvvInput) {
      cvvInput.value = "";
    }
    if (expiryInput) {
      expiryInput.value = "";
    }
  });

  paint();
})();
