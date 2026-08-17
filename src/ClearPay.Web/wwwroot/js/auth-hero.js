(() => {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  if (reduce || typeof anime === "undefined") {
    document.documentElement.classList.remove("auth-motion");
    return;
  }

  document.documentElement.classList.add("auth-motion");

  const reveal = () => document.documentElement.classList.remove("auth-motion");
  try {
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
    window.setTimeout(reveal, 900);
  } catch (err) {
    // #region agent log
    fetch('http://127.0.0.1:7320/ingest/8265b831-5f86-4494-a083-68cbc6788d32',{method:'POST',headers:{'Content-Type':'application/json','X-Debug-Session-Id':'021de0'},body:JSON.stringify({sessionId:'021de0',location:'auth-hero.js',message:'anime failed, revealed login',data:{ok:false,errorType:String(err&&err.name||'Error')},timestamp:Date.now(),hypothesisId:'H'})}).catch(()=>{});
    // #endregion
    reveal();
  }
})();
