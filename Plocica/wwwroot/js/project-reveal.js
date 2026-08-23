(function () {
  var projects = document.querySelectorAll(".project");
  if (!projects.length) return;

  if (!("IntersectionObserver" in window)) {
    projects.forEach(function (el) { el.classList.add("is-visible"); });
    return;
  }

  var observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (entry) {
      if (entry.isIntersecting) {
        entry.target.classList.add("is-visible");
        observer.unobserve(entry.target);
      }
    });
  }, { threshold: 0.15 });

  projects.forEach(function (el) { observer.observe(el); });
})();
