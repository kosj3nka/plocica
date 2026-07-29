(function () {
  var video = document.getElementById("craft-video");
  if (!video) return;

  if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
    video.removeAttribute("autoplay");
    video.pause();
    video.setAttribute("preload", "none");
  }
})();
