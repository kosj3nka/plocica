document.addEventListener("submit", function (e) {
  var form = e.target;
  if (form.matches(".admin-delete-form")) {
    var msg = form.getAttribute("data-confirm") || "Obrisati?";
    if (!confirm(msg)) {
      e.preventDefault();
    }
  }
});

// ---------- Dinamički dodavanje "primjera" (naziv + fotografija) ----------
(function () {
  var addBtn = document.getElementById("add-example-btn");
  var list = document.getElementById("new-example-list");
  var template = document.getElementById("new-example-template");
  if (!addBtn || !list || !template) {
    return;
  }

  function reindex() {
    var rows = list.querySelectorAll(".admin-new-example-row");
    rows.forEach(function (row, i) {
      row.querySelector(".js-new-example-name").name = "Input.NewExamples[" + i + "].Name";
      row.querySelector(".js-new-example-file").name = "Input.NewExamples[" + i + "].ImageFile";
    });
  }

  addBtn.addEventListener("click", function () {
    var row = template.content.firstElementChild.cloneNode(true);
    row.querySelector(".js-remove-example-row").addEventListener("click", function () {
      row.remove();
      reindex();
    });
    list.appendChild(row);
    reindex();
  });
})();
