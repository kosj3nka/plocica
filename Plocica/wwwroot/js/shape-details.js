document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll(".shape-icon-grid").forEach(function (grid) {
    var scope = grid.parentElement;
    var tiles = grid.querySelectorAll(".shape-icon-tile");

    function closeAll() {
      scope.querySelectorAll(".shape-details").forEach(function (panel) {
        panel.hidden = true;
      });
      tiles.forEach(function (tile) {
        tile.setAttribute("aria-expanded", "false");
        tile.classList.remove("is-active");
      });
    }

    tiles.forEach(function (tile) {
      tile.addEventListener("click", function () {
        var panel = document.getElementById(tile.getAttribute("aria-controls"));
        if (!panel) {
          return;
        }
        var wasOpen = !panel.hidden;
        closeAll();
        if (!wasOpen) {
          panel.hidden = false;
          tile.setAttribute("aria-expanded", "true");
          tile.classList.add("is-active");
          panel.scrollIntoView({ behavior: "smooth", block: "nearest" });
        }
      });
    });

    scope.querySelectorAll(".shape-details-close").forEach(function (btn) {
      btn.addEventListener("click", closeAll);
    });
  });
});
