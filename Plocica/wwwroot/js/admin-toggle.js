(function () {
  var radios = document.querySelectorAll('input[type="radio"][data-toggle-target]');
  if (!radios.length) return;

  function update() {
    var selected = document.querySelector('input[type="radio"][data-toggle-target]:checked');
    var target = selected ? selected.getAttribute('data-toggle-target') : null;
    document.querySelectorAll('.admin-toggle-panel').forEach(function (panel) {
      panel.style.display = panel.getAttribute('data-panel') === target ? '' : 'none';
    });
  }

  radios.forEach(function (r) { r.addEventListener('change', update); });
  update();
})();
