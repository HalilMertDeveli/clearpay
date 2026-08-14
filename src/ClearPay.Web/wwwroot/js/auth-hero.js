(() => {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  if (reduce || typeof anime === "undefined") {
    document.documentElement.classList.remove("auth-motion");
    return;
  }

  document.documentElement.classList.add("auth-motion");

  anime
    .timeline({ easing: "easeOutQuad" })
    .add({
      targets: ".auth-orb",
      opacity: [0, 1],
      scale: [0.88, 1],
      duration: 220,
      delay: anime.stagger(70)
    })
    .add(
      {
        targets: ".auth-hero-kicker, .auth-hero-title, .auth-hero-lede",
        opacity: [0, 1],
        translateY: [10, 0],
        duration: 200,
        delay: anime.stagger(45)
      },
      "-=90"
    )
    .add(
      {
        targets: ".auth-card",
        opacity: [0, 1],
        translateX: [18, 0],
        duration: 220
      },
      "-=120"
    )
    .add(
      {
        targets: ".auth-footer",
        opacity: [0, 1],
        duration: 180
      },
      "-=80"
    );
})();
