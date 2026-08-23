document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll("[data-gallery-more]").forEach(function (btn) {
    var gallery = btn.previousElementSibling;
    if (!gallery || !gallery.classList.contains("project-gallery")) {
      return;
    }

    btn.addEventListener("click", function () {
      var expanded = gallery.classList.toggle("is-expanded");
      btn.setAttribute("aria-expanded", expanded ? "true" : "false");
      btn.textContent = expanded ? btn.dataset.labelLess : btn.dataset.labelMore;
    });
  });
});
