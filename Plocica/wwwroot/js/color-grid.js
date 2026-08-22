(function () {
  var grid = document.getElementById("color-grid");
  var popover = document.getElementById("color-popover");
  if (!grid || !popover) return;

  var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
  var token = tokenInput ? tokenInput.value : "";

  var hexInput = document.getElementById("color-popover-hex");
  var codeInput = document.getElementById("color-popover-code");
  var photoInput = document.getElementById("color-popover-photo-input");
  var removePhotoBtn = document.getElementById("color-popover-remove-photo-btn");
  var errorEl = document.getElementById("color-popover-error");

  var activeChip = null;

  function urlFor(name) {
    return grid.getAttribute("data-" + name + "-url");
  }

  function showError(msg) {
    errorEl.textContent = msg || "";
  }

  function renderChip(chip, data) {
    chip.setAttribute("data-hex", data.hex || "");
    chip.setAttribute("data-code", data.code || "");
    chip.setAttribute("data-image-url", data.imageUrl || "");
    var swatch = chip.querySelector('[data-role="swatch"]');
    if (data.imageUrl) {
      swatch.style.background = "";
      swatch.style.backgroundImage = "url('" + data.imageUrl + "')";
    } else {
      swatch.style.backgroundImage = "";
      swatch.style.background = data.hex;
    }
    chip.querySelector('[data-role="code-label"]').textContent = data.code;
  }

  function openPopover(chip) {
    activeChip = chip;
    hexInput.value = chip.getAttribute("data-hex") || "#7C8A5B";
    codeInput.value = chip.getAttribute("data-code") || "";
    var imageUrl = chip.getAttribute("data-image-url");
    removePhotoBtn.hidden = !imageUrl;
    showError("");

    popover.hidden = false;
    var rect = chip.getBoundingClientRect();
    var popW = popover.offsetWidth;
    var left = rect.right + 8;
    if (left + popW > window.innerWidth) {
      left = rect.left - popW - 8;
    }
    popover.style.top = rect.top + "px";
    popover.style.left = left + "px";
  }

  function closePopover() {
    popover.hidden = true;
    activeChip = null;
  }

  async function postForm(url, fields) {
    var body = new FormData();
    body.append("__RequestVerificationToken", token);
    Object.keys(fields).forEach(function (key) {
      body.append(key, fields[key]);
    });
    var res = await fetch(url, { method: "POST", body: body });
    var data = null;
    try {
      data = await res.json();
    } catch (e) {}
    if (!res.ok || !data || !data.ok) {
      throw new Error((data && data.error) || "Greška prilikom spremanja.");
    }
    return data;
  }

  async function saveColor() {
    if (!activeChip) return;
    var chip = activeChip;
    var id = chip.getAttribute("data-color-id");
    try {
      var data = await postForm(urlFor("update"), { id: id, hex: hexInput.value, code: codeInput.value });
      renderChip(chip, data);
      showError("");
    } catch (err) {
      hexInput.value = chip.getAttribute("data-hex");
      codeInput.value = chip.getAttribute("data-code");
      showError(err.message);
    }
  }

  hexInput.addEventListener("change", saveColor);
  codeInput.addEventListener("blur", saveColor);
  codeInput.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      codeInput.blur();
    }
  });

  photoInput.addEventListener("change", async function () {
    if (!activeChip || !photoInput.files.length) return;
    var chip = activeChip;
    var id = chip.getAttribute("data-color-id");
    var body = new FormData();
    body.append("__RequestVerificationToken", token);
    body.append("id", id);
    body.append("file", photoInput.files[0]);
    try {
      var res = await fetch(urlFor("update-image"), { method: "POST", body: body });
      var data = await res.json();
      if (!res.ok || !data.ok) throw new Error((data && data.error) || "Greška prilikom uploada.");
      renderChip(chip, data);
      removePhotoBtn.hidden = false;
      showError("");
    } catch (err) {
      showError(err.message);
    }
    photoInput.value = "";
  });

  removePhotoBtn.addEventListener("click", async function () {
    if (!activeChip) return;
    var chip = activeChip;
    var id = chip.getAttribute("data-color-id");
    try {
      var data = await postForm(urlFor("remove-image"), { id: id });
      renderChip(chip, data);
      removePhotoBtn.hidden = true;
      showError("");
    } catch (err) {
      showError(err.message);
    }
  });

  grid.addEventListener("click", async function (e) {
    var addBtn = e.target.closest('[data-role="add"]');
    if (addBtn) {
      try {
        var data = await postForm(urlFor("add"), {});
        var chip = document.createElement("div");
        chip.className = "admin-color-chip";
        chip.setAttribute("data-color-id", data.id);
        chip.innerHTML =
          '<button type="button" class="admin-color-delete" data-role="delete" title="Obriši">&times;</button>' +
          '<div class="admin-color-swatch" data-role="swatch"></div>' +
          '<span class="admin-color-code" data-role="code-label"></span>';
        grid.insertBefore(chip, addBtn);
        renderChip(chip, data);
        openPopover(chip);
      } catch (err) {
        showError(err.message);
      }
      return;
    }

    var deleteBtn = e.target.closest('[data-role="delete"]');
    if (deleteBtn) {
      var delChip = deleteBtn.closest(".admin-color-chip");
      var code = delChip.getAttribute("data-code");
      if (!confirm('Obrisati boju "' + code + '"?')) return;
      try {
        await postForm(urlFor("delete"), { id: delChip.getAttribute("data-color-id") });
        if (activeChip === delChip) closePopover();
        delChip.remove();
      } catch (err) {
        showError(err.message);
      }
      return;
    }

    var chip = e.target.closest(".admin-color-chip");
    if (chip) {
      openPopover(chip);
    }
  });

  document.addEventListener("click", function (e) {
    if (popover.hidden) return;
    if (popover.contains(e.target) || e.target.closest(".admin-color-chip")) return;
    closePopover();
  });
})();
