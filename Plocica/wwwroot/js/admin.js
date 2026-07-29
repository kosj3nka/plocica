document.addEventListener("submit", function (e) {
  var form = e.target;
  if (form.matches(".admin-delete-form")) {
    var msg = form.getAttribute("data-confirm") || "Obrisati?";
    if (!confirm(msg)) {
      e.preventDefault();
    }
  }
});
