(function () {
  function container(el) {
    return el.closest("[data-admin-upload]");
  }

  function updatePreview(box, files) {
    if (!box || !files || !files.length) return;
    var previewImg = box.querySelector('[data-role="preview-img"]');
    var countEl = box.querySelector('[data-role="file-count"]');
    if (previewImg) {
      if (previewImg.tagName !== "IMG") {
        var img = document.createElement("img");
        img.setAttribute("data-role", "preview-img");
        previewImg.replaceWith(img);
        previewImg = img;
      }
      previewImg.src = URL.createObjectURL(files[0]);
    }
    if (countEl) {
      countEl.textContent = files.length > 1 ? files.length + " datoteka odabrano" : files[0].name;
    }
  }

  document.addEventListener("change", function (e) {
    if (!e.target.matches('[data-role="file-input"]')) return;
    updatePreview(container(e.target), e.target.files);
  });

  ["dragenter", "dragover"].forEach(function (evt) {
    document.addEventListener(evt, function (e) {
      var drop = e.target.closest(".admin-dropzone-drop");
      if (!drop) return;
      e.preventDefault();
      drop.classList.add("is-dragover");
    });
  });

  ["dragleave", "drop"].forEach(function (evt) {
    document.addEventListener(evt, function (e) {
      var drop = e.target.closest(".admin-dropzone-drop");
      if (!drop) return;
      e.preventDefault();
      drop.classList.remove("is-dragover");
    });
  });

  document.addEventListener("drop", function (e) {
    var drop = e.target.closest(".admin-dropzone-drop");
    if (!drop) return;
    var input = drop.querySelector('[data-role="file-input"]');
    var files = e.dataTransfer.files;
    if (input && files && files.length) {
      input.files = files;
      updatePreview(container(drop), files);
    }
  });
})();
