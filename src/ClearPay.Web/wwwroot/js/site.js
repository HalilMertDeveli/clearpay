(() => {
  const toggle = document.querySelector(".nav-toggle");
  const sidebar = document.getElementById("sidebar");
  const backdrop = document.getElementById("nav-backdrop");

  if (toggle && sidebar) {
    const setOpen = (open) => {
      sidebar.classList.toggle("is-open", open);
      toggle.setAttribute("aria-expanded", String(open));
      if (backdrop) {
        backdrop.hidden = !open;
        backdrop.classList.toggle("is-visible", open);
      }
    };

    toggle.addEventListener("click", () => {
      setOpen(!sidebar.classList.contains("is-open"));
    });
    backdrop?.addEventListener("click", () => setOpen(false));
  }

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
      const duration = 720;
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
})();
