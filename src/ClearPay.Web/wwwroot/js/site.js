(() => {
  const toggle = document.querySelector(".nav-toggle");
  const sidebar = document.getElementById("sidebar");
  if (!toggle || !sidebar) {
    return;
  }

  toggle.addEventListener("click", () => {
    const open = sidebar.classList.toggle("is-open");
    toggle.setAttribute("aria-expanded", String(open));
  });
})();
