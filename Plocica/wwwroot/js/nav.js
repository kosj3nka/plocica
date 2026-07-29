(function () {
  var toggle = document.querySelector(".nav-toggle");
  var nav = document.querySelector(".nav-primary");

  if (toggle && nav) {
    toggle.addEventListener("click", function () {
      var isOpen = nav.classList.toggle("is-open");
      toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
    });
  }

  var dropdownItem = document.querySelector(".nav-item-dropdown");
  var dropdownToggle = document.querySelector(".nav-dropdown-toggle");

  if (dropdownItem && dropdownToggle) {
    dropdownToggle.addEventListener("click", function (e) {
      // Na mobitelu: klik otvara/zatvara podizbornik umjesto navigacije.
      if (window.matchMedia("(max-width: 860px)").matches) {
        e.preventDefault();
        var expanded = dropdownItem.getAttribute("aria-expanded") === "true";
        dropdownItem.setAttribute("aria-expanded", expanded ? "false" : "true");
      }
    });

    document.addEventListener("click", function (e) {
      if (!dropdownItem.contains(e.target)) {
        dropdownItem.setAttribute("aria-expanded", "false");
      }
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") {
        dropdownItem.setAttribute("aria-expanded", "false");
      }
    });
  }
})();
