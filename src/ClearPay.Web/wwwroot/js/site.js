(() => {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const lang = document.documentElement.lang || "tr";
  const locale = { tr: "tr-TR", en: "en-US", de: "de-DE", fr: "fr-FR" }[lang] || "tr-TR";

  const formatTry = (amount) =>
    new Intl.NumberFormat(locale, { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(amount) + " ₺";

  if (!reduce) {
    document.querySelectorAll("[data-count]").forEach((el) => {
      const target = Number(el.getAttribute("data-count"));
      if (!Number.isFinite(target)) {
        return;
      }
      const duration = 240;
      const start = performance.now();
      const tick = (now) => {
        const t = Math.min(1, (now - start) / duration);
        const eased = 1 - Math.pow(1 - t, 3);
        el.textContent = formatTry(target * eased);
        if (t < 1) {
          requestAnimationFrame(tick);
        }
      };
      requestAnimationFrame(tick);
    });
  }

  const clock = document.querySelector(".live-clock");
  if (clock) {
    const write = () => {
      clock.textContent = new Intl.DateTimeFormat(locale, {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit"
      }).format(new Date());
    };
    write();
    if (!reduce) {
      setInterval(write, 1000);
    }
  }

  document.querySelectorAll('form[method="post"]').forEach((form) => {
    if (form.closest(".lang-picker") || form.classList.contains("topbar-form")) {
      return;
    }

    form.addEventListener("submit", (event) => {
      if (form.getAttribute("aria-busy") === "true") {
        event.preventDefault();
        return;
      }

      form.setAttribute("aria-busy", "true");
      form.querySelectorAll('button[type="submit"]').forEach((btn) => {
        btn.classList.add("is-busy");
      });
    });
  });

  document.querySelectorAll("[data-copy]").forEach((btn) => {
    btn.addEventListener("click", async () => {
      const text = btn.getAttribute("data-copy") || "";
      if (!text || !navigator.clipboard) {
        return;
      }

      try {
        await navigator.clipboard.writeText(text);
        const done = btn.getAttribute("data-copied") || "";
        const original = btn.textContent;
        if (done) {
          btn.textContent = done;
          window.setTimeout(() => {
            btn.textContent = original;
          }, 1600);
        }
      } catch {
        /* clipboard may be blocked */
      }
    });
  });

  document.querySelectorAll("[data-print]").forEach((btn) => {
    btn.addEventListener("click", () => window.print());
  });
})();
