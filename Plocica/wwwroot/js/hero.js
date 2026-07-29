(function () {
  var frame = document.querySelector(".plate-frame");
  if (!frame) return;

  var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  var frames = frame.querySelectorAll(".hero-frame");

  function revealContainer() {
    setTimeout(function () {
      frame.classList.add("is-revealed");
    }, 30);
  }

  var firstImg = frames[0];
  if (firstImg && firstImg.complete) {
    revealContainer();
  } else if (firstImg) {
    firstImg.addEventListener("load", revealContainer, { once: true });
  } else {
    revealContainer();
  }

  if (!frames.length) return;

  if (reduceMotion) {
    frames.forEach(function (f) { f.classList.remove("is-active"); });
    frames[frames.length - 1].classList.add("is-active");
    return;
  }

  var current = 0;
  setInterval(function () {
    frames[current].classList.remove("is-active");
    current = (current + 1) % frames.length;
    frames[current].classList.add("is-active");
  }, 3000);
})();
