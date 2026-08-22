# Admin Panel UI/UX Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rebuild the admin panel's navigation, action buttons, image upload/preview, and the Boje (colors) page into a coherent, icon-literate, direct-manipulation UI, per the approved design spec.

**Architecture:** A left sidebar replaces the top nav bar. A small set of new Razor partials (`_AdminIcon`, `_AdminActionButtons`, `_AdminImageUpload`, `_AdminImageRow`) get reused across Kolekcija/Boje/Projekti to eliminate today's duplicated table/form markup. Boje moves from list+edit pages to one direct-edit grid page backed by new small AJAX page handlers — the only place in the admin panel that moves off full-page POST/redirect.

**Tech Stack:** ASP.NET Core 8 Razor Pages, EF Core (SQL Server), vanilla CSS (`admin.css`, no framework), vanilla JS (no build step, no bundler), existing `IBlobStorageService` for image storage.

**Spec:** `docs/superpowers/specs/2026-08-22-admin-panel-uiux-redesign-design.md`

## Global Constraints

- No new CSS framework or icon library/font — icons are hand-authored inline SVG, styled with `currentColor`.
- No new JS dependency, no build step/bundler — plain delegated-listener scripts matching `admin.js`/`admin-toggle.js`'s existing style.
- Delete stays behind the native browser `confirm()` — no custom modal.
- No drag-and-drop reordering anywhere (Kolekcija/Projekti image rows keep numeric `SortOrder` inputs; the Boje grid has no reorder UI).
- No public-site changes (`Kolekcije.cshtml`, `_ShapeGrid.cshtml`) — this plan is admin-only.
- **No test project exists in this repo** (single `Plocica.csproj`, no `*Tests*` project). Every task's "test" step is `dotnet build` (compiles) plus a concrete manual browser check — there is no red/green unit test cycle to run. Follow the manual-check wording literally; it replaces automated tests for this plan.
- Commit after each task with the repo's existing commit style (`feat:`/`style:`/`docs:` prefix, as seen in recent commits).

---

## File Structure

New files:
- `Plocica/Pages/Shared/_AdminIcon.cshtml` — inline SVG icon partial, `@model string` (icon name).
- `Plocica/Pages/Shared/_AdminNav.cshtml` — sidebar nav partial.
- `Plocica/Pages/Shared/AdminActionButtonsViewModel.cs` + `_AdminActionButtons.cshtml` — Edit/Delete row-action partial.
- `Plocica/Pages/Shared/AdminImageUploadViewModel.cs` + `_AdminImageUpload.cshtml` — dropzone-with-preview partial.
- `Plocica/Pages/Shared/AdminImageRowViewModel.cs` + `_AdminImageRow.cshtml` — existing-image row partial.
- `Plocica/wwwroot/js/admin-upload.js` — delegated dropzone/preview behavior.
- `Plocica/wwwroot/js/color-grid.js` — Boje grid popover + AJAX.

Modified files:
- `Plocica/Pages/Shared/_AdminLayout.cshtml` — sidebar layout shell.
- `Plocica/wwwroot/css/admin.css` — sidebar, icon-btn, dropzone, color-grid, image-row styles.
- `Plocica/Pages/Admin/Kolekcija/Index.cshtml`, `Edit.cshtml` — shared partials wired in.
- `Plocica/Pages/Admin/Projekti/Index.cshtml`, `Edit.cshtml` — shared partials wired in.
- `Plocica/Pages/Admin/Index.cshtml` — dashboard card icons.
- `Plocica/Pages/Admin/Login.cshtml` — icon polish.
- `Plocica/Pages/Admin/Boje/Index.cshtml`, `Index.cshtml.cs` — rewritten as the direct-edit grid.
- `Plocica/wwwroot/js/admin.js` — unchanged logic, only the delete-button icon markup around it changes (handled via the new `_AdminActionButtons` partial, not this file).

Deleted files:
- `Plocica/Pages/Admin/Boje/Edit.cshtml`
- `Plocica/Pages/Admin/Boje/Edit.cshtml.cs`

---

### Task 1: Shared icon partial

**Files:**
- Create: `Plocica/Pages/Shared/_AdminIcon.cshtml`

**Interfaces:**
- Produces: `<partial name="_AdminIcon" model="@("edit")" />` (and `"delete"`, `"save"`, `"cancel"`, `"plus"`, `"upload"`, `"dashboard"`, `"kolekcija"`, `"boje"`, `"projekti"`, `"logout"`, `"external-link"`, `"camera"`) — renders a `<svg width="1em" height="1em" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">...</svg>` for the named icon, or nothing for an unrecognized name.

- [ ] **Step 1: Write the partial**

```cshtml
@model string
@{
    var attrs = "fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"";
}
@switch (Model)
{
    case "edit":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>
        break;
    case "delete":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M3 6h18M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2m3 0-1 14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2L4 6"/></svg>
        break;
    case "save":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/><path d="M17 21v-8H7v8M7 3v5h8"/></svg>
        break;
    case "cancel":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M18 6 6 18M6 6l12 12"/></svg>
        break;
    case "plus":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M12 5v14M5 12h14"/></svg>
        break;
    case "upload":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>
        break;
    case "dashboard":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>
        break;
    case "kolekcija":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M4 4h16v4H4zM4 10h16v10H4z"/></svg>
        break;
    case "boje":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><circle cx="12" cy="12" r="9"/><circle cx="12" cy="12" r="3"/></svg>
        break;
    case "projekti":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M3 9l9-6 9 6v11a1 1 0 0 1-1 1h-5v-7H9v7H4a1 1 0 0 1-1-1z"/></svg>
        break;
    case "logout":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><path d="M16 17l5-5-5-5"/><path d="M21 12H9"/></svg>
        break;
    case "external-link":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/><path d="M15 3h6v6"/><path d="M10 14 21 3"/></svg>
        break;
    case "camera":
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="admin-icon"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>
        break;
}
```

- [ ] **Step 2: Add the icon's base size to `admin.css`**

Append to `Plocica/wwwroot/css/admin.css`:

```css
/* ---------- Ikone ---------- */

.admin-icon { width: 1em; height: 1em; flex-shrink: 0; }
```

- [ ] **Step 3: Build**

Run: `dotnet build Plocica/Plocica.csproj`
Expected: builds with no errors (a `@model string` partial with a `@switch` is valid Razor — no runtime check possible yet since nothing references it).

- [ ] **Step 4: Commit**

```bash
git add Plocica/Pages/Shared/_AdminIcon.cshtml Plocica/wwwroot/css/admin.css
git commit -m "feat: add shared admin icon partial"
```

---

### Task 2: Sidebar navigation

**Files:**
- Create: `Plocica/Pages/Shared/_AdminNav.cshtml`
- Modify: `Plocica/Pages/Shared/_AdminLayout.cshtml`
- Modify: `Plocica/wwwroot/css/admin.css`

**Interfaces:**
- Consumes: `_AdminIcon` partial (Task 1).
- Produces: `<partial name="_AdminNav" />`, included from `_AdminLayout.cshtml`'s authenticated branch.

- [ ] **Step 1: Write the sidebar partial**

```cshtml
@{
    var path = Context.Request.Path.Value ?? string.Empty;
    bool IsActive(string prefix) => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
}
<nav class="admin-sidebar">
    <a class="admin-sidebar-brand" asp-page="/Admin/Index">Pločica <span>admin</span></a>

    <a class="admin-sidebar-item @(IsActive("/Admin/Kolekcija") ? "active" : "")" asp-page="/Admin/Kolekcija/Index">
        <partial name="_AdminIcon" model="@("kolekcija")" />
        Kolekcija
    </a>
    <a class="admin-sidebar-item @(IsActive("/Admin/Boje") ? "active" : "")" asp-page="/Admin/Boje/Index">
        <partial name="_AdminIcon" model="@("boje")" />
        Boje
    </a>
    <a class="admin-sidebar-item @(IsActive("/Admin/Projekti") ? "active" : "")" asp-page="/Admin/Projekti/Index">
        <partial name="_AdminIcon" model="@("projekti")" />
        Projekti
    </a>

    <div class="admin-sidebar-spacer"></div>

    <a href="/" target="_blank" rel="noopener" class="admin-sidebar-item admin-sidebar-view-site">
        <partial name="_AdminIcon" model="@("external-link")" />
        Pogledaj stranicu
    </a>
    <form method="post" asp-page="/Admin/Logout" class="admin-sidebar-logout-form">
        <button type="submit" class="admin-sidebar-item admin-sidebar-logout-btn">
            <partial name="_AdminIcon" model="@("logout")" />
            Odjava
        </button>
    </form>
</nav>
```

- [ ] **Step 2: Replace the top-bar layout with a sidebar layout shell**

In `Plocica/Pages/Shared/_AdminLayout.cshtml`, replace the `<body>` block:

```cshtml
<body class="admin-body">
    @if (User.Identity?.IsAuthenticated == true)
    {
        <div class="admin-shell">
            <partial name="_AdminNav" />
            <main class="admin-main">
                @RenderBody()
            </main>
        </div>
    }
    else
    {
        <main class="admin-main">
            @RenderBody()
        </main>
    }

    @await RenderSectionAsync("Scripts", required: false)
</body>
```

(The `else` branch keeps the Login page — which renders before authentication — working without a sidebar.)

- [ ] **Step 3: Replace the old header CSS with sidebar CSS**

In `Plocica/wwwroot/css/admin.css`, delete the `.admin-header`, `.admin-header-inner`, `.admin-brand`, `.admin-nav`, `.admin-view-site` rules (lines 11–50 as read — the old top-bar chrome, fully superseded) and the standalone `.admin-logout-btn` rule (lines 52–61), then replace `.admin-main`'s old centered-content rule with:

```css
.admin-shell {
  display: flex;
  min-height: 100vh;
}

.admin-sidebar {
  width: 220px;
  flex-shrink: 0;
  background: var(--ink);
  color: var(--paper);
  display: flex;
  flex-direction: column;
  padding: 1.25rem 0;
  position: sticky;
  top: 0;
  height: 100vh;
}

.admin-sidebar-brand {
  font-family: var(--font-display);
  font-weight: 700;
  font-size: 1rem;
  padding: 0 1.25rem 1rem;
  margin-bottom: 0.75rem;
  border-bottom: 1px solid rgba(242, 240, 234, 0.15);
}

.admin-sidebar-brand span { color: var(--paper-2); font-weight: 400; opacity: 0.7; }

.admin-sidebar-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.7em 1.25rem;
  font-size: 0.9375rem;
  color: var(--paper-2);
  border-left: 3px solid transparent;
  opacity: 0.85;
  background: none;
  border-top: none;
  border-right: none;
  border-bottom: none;
  font: inherit;
  text-align: left;
  cursor: pointer;
  width: 100%;
}

.admin-sidebar-item:hover { opacity: 1; }

.admin-sidebar-item.active {
  background: rgba(227, 178, 60, 0.12);
  border-left-color: var(--glz-sun);
  color: var(--paper);
  opacity: 1;
  font-weight: 700;
}

.admin-sidebar-spacer { flex: 1; }

.admin-sidebar-view-site,
.admin-sidebar-logout-btn { font-size: 0.8125rem; }

.admin-sidebar-logout-form { border-top: 1px solid rgba(242, 240, 234, 0.15); margin-top: 0.5rem; padding-top: 0.5rem; }

@media (max-width: 860px) {
  .admin-sidebar { width: 64px; }
  .admin-sidebar-brand,
  .admin-sidebar-item span,
  .admin-sidebar-item:not(.admin-sidebar-logout-btn) { }
  .admin-sidebar-brand { padding: 0 0 1rem; text-align: center; font-size: 0; }
  .admin-sidebar-brand::before { content: "P"; font-size: 1.1rem; }
  .admin-sidebar-item { justify-content: center; padding: 0.7em 0; }
  .admin-sidebar-item:not(:has(.admin-icon)) { display: none; }
}

.admin-main {
  flex: 1;
  padding: 2rem 2.5rem 4rem;
  min-width: 0;
}

.admin-container h1 { font-size: 1.75rem; margin: 0 0 1.5rem; }
```

(The `@media` collapse hides text labels below `860px` by shrinking the sidebar to icon width; text nodes naturally wrap/clip since the container is narrower — this is an acceptable simple collapse per the spec's "icon-only rail" requirement without extra JS.)

- [ ] **Step 4: Build and manually verify**

Run: `dotnet build Plocica/Plocica.csproj`
Then run: `dotnet run --project Plocica/Plocica.csproj`, log into `/Admin/Login`, and open `/Admin/Boje` (or any admin page).
Expected: a dark left sidebar with brand/logo linking to the dashboard, three icon+label links, the current page's link showing a gold left bar, and "Pogledaj stranicu" + "Odjava" at the bottom. Resize the window to ~800px and confirm the sidebar shrinks to an icon-only rail without breaking the page layout.

- [ ] **Step 5: Commit**

```bash
git add Plocica/Pages/Shared/_AdminNav.cshtml Plocica/Pages/Shared/_AdminLayout.cshtml Plocica/wwwroot/css/admin.css
git commit -m "feat: replace admin top nav with sidebar, add active-page state"
```

---

### Task 3: Shared row-action buttons (Edit/Delete)

**Files:**
- Create: `Plocica/Pages/Shared/AdminActionButtonsViewModel.cs`
- Create: `Plocica/Pages/Shared/_AdminActionButtons.cshtml`
- Modify: `Plocica/wwwroot/css/admin.css`
- Modify: `Plocica/Pages/Admin/Kolekcija/Index.cshtml`
- Modify: `Plocica/Pages/Admin/Projekti/Index.cshtml`

**Interfaces:**
- Consumes: `_AdminIcon` partial (Task 1).
- Produces: `<partial name="_AdminActionButtons" model="@(new AdminActionButtonsViewModel { EditRouteId = shape.Id, DeleteHandler = "Delete", DeleteRouteId = shape.Id, ConfirmText = $"Obrisati oblik \"{shape.Name}\"?" })" />`

- [ ] **Step 1: Write the view model**

```csharp
namespace Plocica.Pages.Shared;

public class AdminActionButtonsViewModel
{
    public int EditRouteId { get; set; }
    public string DeleteHandler { get; set; } = "Delete";
    public int DeleteRouteId { get; set; }
    public string ConfirmText { get; set; } = "Obrisati?";
}
```

- [ ] **Step 2: Write the partial**

```cshtml
@model Plocica.Pages.Shared.AdminActionButtonsViewModel
<div class="admin-row-actions">
    <a asp-page="Edit" asp-route-id="@Model.EditRouteId" class="icon-btn">
        <partial name="_AdminIcon" model="@("edit")" />
        Uredi
    </a>
    <form method="post" asp-page-handler="@Model.DeleteHandler" asp-route-id="@Model.DeleteRouteId"
          class="admin-delete-form" data-confirm="@Model.ConfirmText">
        <button type="submit" class="icon-btn icon-btn-danger">
            <partial name="_AdminIcon" model="@("delete")" />
            Obriši
        </button>
    </form>
</div>
```

- [ ] **Step 3: Add icon-btn styles to `admin.css`, replacing `.admin-delete-btn`'s bare-link look**

Replace the existing `.admin-delete-btn` rule with:

```css
.icon-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4em;
  padding: 0.45em 0.85em;
  border: 1px solid var(--line);
  border-radius: 6px;
  background: transparent;
  color: var(--ink);
  font: inherit;
  font-size: 0.875rem;
  cursor: pointer;
  text-decoration: none;
  transition: border-color var(--dur-fast) var(--ease-spring), background var(--dur-fast) var(--ease-spring);
}

.icon-btn:hover,
.icon-btn:focus-visible { border-color: var(--ink); background: var(--paper-2); }

.icon-btn-danger { border-color: var(--glz-clay); color: var(--glz-clay); }
.icon-btn-danger:hover,
.icon-btn-danger:focus-visible { background: rgba(192, 107, 74, 0.08); border-color: var(--glz-clay); }

.icon-btn-primary { background: var(--ink); color: var(--paper); border-color: var(--ink); }
.icon-btn-primary:hover,
.icon-btn-primary:focus-visible { background: color-mix(in srgb, var(--ink) 88%, white); border-color: color-mix(in srgb, var(--ink) 88%, white); }
```

- [ ] **Step 4: Wire into Kolekcija/Index.cshtml**

In `Plocica/Pages/Admin/Kolekcija/Index.cshtml`, replace each of the three `<td class="admin-row-actions">...</td>` blocks (Oblici, Ručno oslikane, Reljefne loops) with:

```cshtml
<td>
    <partial name="_AdminActionButtons" model="@(new AdminActionButtonsViewModel { EditRouteId = shape.Id, DeleteRouteId = shape.Id, ConfirmText = $"Obrisati oblik \"{shape.Name}\"?" })" />
</td>
```

(Reljefne's confirm text in the current markup reads `"Obrisati stavku ..."` — keep that exact wording for that loop's instance, `"Obrisati oblik ..."` for the other two, matching today's copy.)

Add `@using Plocica.Pages.Shared` to the top of the file (below `@model`).

- [ ] **Step 5: Wire into Projekti/Index.cshtml**

In `Plocica/Pages/Admin/Projekti/Index.cshtml`, replace the `<td class="admin-row-actions">...</td>` block with:

```cshtml
<td>
    <partial name="_AdminActionButtons" model="@(new AdminActionButtonsViewModel { EditRouteId = project.Id, DeleteRouteId = project.Id, ConfirmText = $"Obrisati projekt \"{project.Title}\"?" })" />
</td>
```

Add `@using Plocica.Pages.Shared` to the top of the file.

- [ ] **Step 6: Build and manually verify**

Run: `dotnet build Plocica/Plocica.csproj`
Then in the browser, open `/Admin/Kolekcija` and `/Admin/Projekti`.
Expected: every row shows an icon+label "Uredi" button and a clay-colored icon+label "Obriši" button (not a bare underlined link); clicking Obriši still shows the native confirm dialog with the same message as before.

- [ ] **Step 7: Commit**

```bash
git add Plocica/Pages/Shared/AdminActionButtonsViewModel.cs Plocica/Pages/Shared/_AdminActionButtons.cshtml Plocica/wwwroot/css/admin.css Plocica/Pages/Admin/Kolekcija/Index.cshtml Plocica/Pages/Admin/Projekti/Index.cshtml
git commit -m "feat: add icon-labeled edit/delete buttons, replace bare delete link"
```

---

### Task 4: Image upload dropzone with live preview

**Files:**
- Create: `Plocica/Pages/Shared/AdminImageUploadViewModel.cs`
- Create: `Plocica/Pages/Shared/_AdminImageUpload.cshtml`
- Create: `Plocica/wwwroot/js/admin-upload.js`
- Modify: `Plocica/wwwroot/css/admin.css`
- Modify: `Plocica/Pages/Admin/Kolekcija/Edit.cshtml`
- Modify: `Plocica/Pages/Admin/Projekti/Edit.cshtml`

**Interfaces:**
- Consumes: `_AdminIcon` partial (Task 1).
- Produces: `<partial name="_AdminImageUpload" model="@(new AdminImageUploadViewModel { InputName = "Input.ImageFile", ExistingImageUrl = Model.ExistingImageUrl })" />` — renders a dropzone; the underlying `<input type="file" name="...">` still participates in normal ASP.NET model binding, so call sites keep their existing `asp-validation-for="Input.ImageFile"` span immediately after the partial, unchanged.

- [ ] **Step 1: Write the view model**

```csharp
namespace Plocica.Pages.Shared;

public class AdminImageUploadViewModel
{
    public string InputName { get; set; } = default!;
    public string? ExistingImageUrl { get; set; }
    public string Accept { get; set; } = ".jpg,.jpeg,.png,.webp";
    public bool Multiple { get; set; }
}
```

- [ ] **Step 2: Write the partial**

```cshtml
@model Plocica.Pages.Shared.AdminImageUploadViewModel
<div class="admin-dropzone" data-admin-upload>
    <div class="admin-dropzone-thumb">
        @if (!string.IsNullOrEmpty(Model.ExistingImageUrl))
        {
            <img src="@Model.ExistingImageUrl" alt="" data-role="preview-img" />
        }
        else
        {
            <span class="admin-dropzone-thumb-empty" data-role="preview-img"><partial name="_AdminIcon" model="@("upload")" /></span>
        }
    </div>
    <label class="admin-dropzone-drop">
        <input type="file" name="@Model.InputName" accept="@Model.Accept" data-role="file-input" multiple="@(Model.Multiple ? "multiple" : null)" />
        <span class="admin-dropzone-text"><b>Povuci sliku ovdje</b> ili klikni za odabir</span>
        <span class="admin-dropzone-count" data-role="file-count"></span>
    </label>
</div>
```

- [ ] **Step 3: Write the delegated JS behavior**

```js
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
```

Because these listeners are delegated on `document`, dropzones inside the Kolekcija "Primjeri" dynamically-cloned template (Task 4, Step 6 below) work with no extra init call.

- [ ] **Step 4: Add dropzone CSS**

Append to `Plocica/wwwroot/css/admin.css`:

```css
/* ---------- Upload dropzone ---------- */

.admin-dropzone {
  display: flex;
  align-items: center;
  gap: 1rem;
  max-width: 480px;
}

.admin-dropzone-thumb {
  width: 72px;
  height: 72px;
  flex-shrink: 0;
  border-radius: 6px;
  overflow: hidden;
  background: var(--paper-2);
  display: flex;
  align-items: center;
  justify-content: center;
}

.admin-dropzone-thumb img { width: 100%; height: 100%; object-fit: cover; }
.admin-dropzone-thumb-empty { color: var(--ink-soft); }
.admin-dropzone-thumb-empty .admin-icon { width: 1.75em; height: 1.75em; }

.admin-dropzone-drop {
  flex: 1;
  border: 2px dashed var(--line);
  border-radius: 8px;
  padding: 0.9em 1em;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 0.2em;
  transition: border-color var(--dur-fast) var(--ease-spring), background var(--dur-fast) var(--ease-spring);
}

.admin-dropzone-drop:hover,
.admin-dropzone-drop.is-dragover { border-color: var(--glz-indigo); background: rgba(46, 58, 86, 0.04); }

.admin-dropzone-drop input[type="file"] { position: absolute; width: 1px; height: 1px; opacity: 0; overflow: hidden; }

.admin-dropzone-text { font-size: 0.875rem; color: var(--ink-soft); }
.admin-dropzone-text b { color: var(--ink); }
.admin-dropzone-count { font-size: 0.75rem; color: var(--ink-soft); }
```

- [ ] **Step 5: Wire into Kolekcija/Edit.cshtml's two single-image fields and two multi-file fields**

In `Plocica/Pages/Admin/Kolekcija/Edit.cshtml`, replace the "Skica" field's `<input asp-for="Input.ImageFile" ... />` block with:

```cshtml
<div class="admin-field">
    <label>Skica (ikona u mreži oblika)</label>
    <partial name="_AdminImageUpload" model="@(new AdminImageUploadViewModel { InputName = "Input.ImageFile", ExistingImageUrl = Model.ExistingImageUrl })" />
    <span asp-validation-for="Input.ImageFile" class="admin-field-error"></span>
    <p class="admin-hint">JPG, PNG ili WEBP, do 5 MB.</p>
</div>
```

Replace the "Opća fotografija" field the same way with `InputName = "Input.PhotoFile", ExistingImageUrl = Model.ExistingPhotoUrl`.

Replace the "Shema slaganja" new-files field (`Input.NewLayoutImageFiles`) and "Reljefne fotografije" new-files field (`Input.NewReljefneImageFiles`) with:

```cshtml
<div class="admin-field">
    <label>Dodaj fotografije</label>
    <partial name="_AdminImageUpload" model="@(new AdminImageUploadViewModel { InputName = "Input.NewLayoutImageFiles", Multiple = true })" />
    <span asp-validation-for="Input.NewLayoutImageFiles" class="admin-field-error"></span>
    <p class="admin-hint">JPG, PNG ili WEBP, do 5 MB po slici. Moguć višestruki odabir.</p>
</div>
```

(and the `NewReljefneImageFiles` equivalent). Add `@using Plocica.Pages.Shared` to the top of the file.

- [ ] **Step 6: Convert the "Primjeri" template's file input to the same dropzone markup**

In the `<template id="new-example-template">` block, replace:

```cshtml
<div class="admin-example-field">
    <label>Fotografija</label>
    <input type="file" accept=".jpg,.jpeg,.png,.webp" class="js-new-example-file" />
</div>
```

with:

```cshtml
<div class="admin-example-field">
    <label>Fotografija</label>
    <div class="admin-dropzone" data-admin-upload>
        <div class="admin-dropzone-thumb">
            <span class="admin-dropzone-thumb-empty" data-role="preview-img"><partial name="_AdminIcon" model="@("upload")" /></span>
        </div>
        <label class="admin-dropzone-drop">
            <input type="file" accept=".jpg,.jpeg,.png,.webp" class="js-new-example-file" data-role="file-input" />
            <span class="admin-dropzone-text"><b>Povuci</b> ili klikni</span>
        </label>
    </div>
</div>
```

`admin.js`'s existing `reindex()` function still finds this input via `.js-new-example-file` (that class is untouched) — only the visual wrapper changed, so the add/remove/reindex logic needs no code change.

- [ ] **Step 7: Wire into Projekti/Edit.cshtml's gallery field**

Replace the "Dodaj slike" field's `<input asp-for="Input.NewImageFiles" ... />` block with:

```cshtml
<div class="admin-field">
    <label>Dodaj slike</label>
    <partial name="_AdminImageUpload" model="@(new AdminImageUploadViewModel { InputName = "Input.NewImageFiles", Multiple = true })" />
    <span asp-validation-for="Input.NewImageFiles" class="admin-field-error"></span>
    <p class="admin-hint">JPG, PNG ili WEBP, do 5 MB po slici. Moguć višestruki odabir.</p>
</div>
```

Add `@using Plocica.Pages.Shared` to the top of the file. Add `<script src="~/js/admin-upload.js" asp-append-version="true"></script>` to the `@section Scripts` block in Kolekcija/Edit.cshtml and Projekti/Edit.cshtml (Projekti/Edit currently has no `@section Scripts` block — add one).

- [ ] **Step 8: Build and manually verify**

Run: `dotnet build Plocica/Plocica.csproj`
Then open `/Admin/Kolekcija/Edit` for an existing Oblici item, and `/Admin/Projekti/Edit` for an existing project.
Expected: each image field shows a dropzone with the existing image (or an empty upload-icon placeholder for new items); selecting a new file instantly swaps in a preview of that file (test by picking a different image than what's shown); dragging a file onto the dropzone also works; the multi-file "Dodaj primjer" flow (click "+ Dodaj primjer") produces a dropzone-styled photo picker per new row.

- [ ] **Step 9: Commit**

```bash
git add Plocica/Pages/Shared/AdminImageUploadViewModel.cs Plocica/Pages/Shared/_AdminImageUpload.cshtml Plocica/wwwroot/js/admin-upload.js Plocica/wwwroot/css/admin.css Plocica/Pages/Admin/Kolekcija/Edit.cshtml Plocica/Pages/Admin/Projekti/Edit.cshtml
git commit -m "feat: add drag-drop image upload with live preview"
```

---

### Task 5: Shared existing-image row list

**Files:**
- Create: `Plocica/Pages/Shared/AdminImageRowViewModel.cs`
- Create: `Plocica/Pages/Shared/_AdminImageRow.cshtml`
- Modify: `Plocica/wwwroot/css/admin.css`
- Modify: `Plocica/Pages/Admin/Kolekcija/Edit.cshtml`
- Modify: `Plocica/Pages/Admin/Projekti/Edit.cshtml`

**Interfaces:**
- Consumes: `_AdminIcon` partial (Task 1).
- Produces: `<partial name="_AdminImageRow" model="@(new AdminImageRowViewModel { ... })" />` — one row of the repeated "existing image: thumb + optional name + order + remove" block. Field names are passed as plain strings so the rendered `<input name="...">` still binds into the page's existing `Input.Existing*[i].*` model-binding paths — no change to any page-model class.

- [ ] **Step 1: Write the view model**

```csharp
namespace Plocica.Pages.Shared;

public class AdminImageRowViewModel
{
    public string ImageUrl { get; set; } = default!;
    public int Id { get; set; }
    public string IdInputName { get; set; } = default!;
    public string ImageUrlInputName { get; set; } = default!;
    public int SortOrder { get; set; }
    public string SortOrderInputName { get; set; } = default!;
    public string DeleteInputName { get; set; } = default!;
    public string? NameInputName { get; set; }
    public string? Name { get; set; }
    public bool ShowSortOrder { get; set; } = true;
}
```

- [ ] **Step 2: Write the partial**

```cshtml
@model Plocica.Pages.Shared.AdminImageRowViewModel
<div class="admin-image-row">
    <img src="@Model.ImageUrl" alt="" class="admin-thumb-preview" />
    <input type="hidden" name="@Model.IdInputName" value="@Model.Id" />
    <input type="hidden" name="@Model.ImageUrlInputName" value="@Model.ImageUrl" />
    @if (Model.NameInputName is not null)
    {
        <div class="admin-example-field">
            <label>Naziv</label>
            <input name="@Model.NameInputName" value="@Model.Name" />
        </div>
    }
    @if (Model.ShowSortOrder)
    {
        <label class="admin-image-sort">
            Redoslijed
            <input type="number" name="@Model.SortOrderInputName" value="@Model.SortOrder" />
        </label>
    }
    <label class="admin-image-delete">
        <input type="checkbox" name="@Model.DeleteInputName" value="true" />
        <partial name="_AdminIcon" model="@("delete")" />
        Obriši
    </label>
</div>
```

- [ ] **Step 3: Give the delete icon inside the row proper alignment**

Append to `Plocica/wwwroot/css/admin.css`:

```css
.admin-image-delete .admin-icon { color: var(--glz-clay); }
```

- [ ] **Step 4: Wire into Kolekcija/Edit.cshtml's three existing-image loops**

Replace the "Shema slaganja" loop body:

```cshtml
@for (var i = 0; i < Model.Input.ExistingLayoutImages.Count; i++)
{
    <partial name="_AdminImageRow" model="@(new AdminImageRowViewModel {
        ImageUrl = Model.Input.ExistingLayoutImages[i].ImageUrl,
        Id = Model.Input.ExistingLayoutImages[i].Id,
        IdInputName = $"Input.ExistingLayoutImages[{i}].Id",
        ImageUrlInputName = $"Input.ExistingLayoutImages[{i}].ImageUrl",
        SortOrder = Model.Input.ExistingLayoutImages[i].SortOrder,
        SortOrderInputName = $"Input.ExistingLayoutImages[{i}].SortOrder",
        DeleteInputName = $"Input.ExistingLayoutImages[{i}].Delete",
    })" />
}
```

Replace the "Primjeri" loop body the same way, adding the name field. `ExistingExampleInput` (defined in this file) has no `SortOrder` property, so pass `ShowSortOrder = false`:

```cshtml
@for (var i = 0; i < Model.Input.ExistingExamples.Count; i++)
{
    <partial name="_AdminImageRow" model="@(new AdminImageRowViewModel {
        ImageUrl = Model.Input.ExistingExamples[i].ImageUrl,
        Id = Model.Input.ExistingExamples[i].Id,
        IdInputName = $"Input.ExistingExamples[{i}].Id",
        ImageUrlInputName = $"Input.ExistingExamples[{i}].ImageUrl",
        DeleteInputName = $"Input.ExistingExamples[{i}].Delete",
        NameInputName = $"Input.ExistingExamples[{i}].Name",
        Name = Model.Input.ExistingExamples[i].Name,
        ShowSortOrder = false,
    })" />
}
```

Replace the "Reljefne fotografije" loop body the same way as "Shema slaganja" (using `ExistingReljefneImages`).

- [ ] **Step 5: Wire into Projekti/Edit.cshtml's existing-images loop**

```cshtml
@for (var i = 0; i < Model.Input.ExistingImages.Count; i++)
{
    <partial name="_AdminImageRow" model="@(new AdminImageRowViewModel {
        ImageUrl = Model.Input.ExistingImages[i].Url,
        Id = Model.Input.ExistingImages[i].Id,
        IdInputName = $"Input.ExistingImages[{i}].Id",
        ImageUrlInputName = $"Input.ExistingImages[{i}].Url",
        SortOrder = Model.Input.ExistingImages[i].SortOrder,
        SortOrderInputName = $"Input.ExistingImages[{i}].SortOrder",
        DeleteInputName = $"Input.ExistingImages[{i}].Delete",
    })" />
}
```

Note the property is `Url` here, not `ImageUrl` — the view model's `ImageUrlInputName` just needs to point at whatever the page's binding path actually is; the partial itself is agnostic to the property's real name.

- [ ] **Step 6: Build and manually verify**

Run: `dotnet build Plocica/Plocica.csproj`
Then open `/Admin/Kolekcija/Edit` for an item that has layout images and examples, and `/Admin/Projekti/Edit` for a project with images.
Expected: existing image rows render identically to before (thumb, sort/name fields, delete checkbox with a trash icon), and saving the form after ticking a delete checkbox or changing a sort number still works exactly as before (this is a pure markup refactor — the field names/values posted are unchanged).

- [ ] **Step 7: Commit**

```bash
git add Plocica/Pages/Shared/AdminImageRowViewModel.cs Plocica/Pages/Shared/_AdminImageRow.cshtml Plocica/wwwroot/css/admin.css Plocica/Pages/Admin/Kolekcija/Edit.cshtml Plocica/Pages/Admin/Projekti/Edit.cshtml
git commit -m "refactor: extract shared existing-image row partial"
```

---

### Task 6: Login and Dashboard polish

**Files:**
- Modify: `Plocica/Pages/Admin/Login.cshtml`
- Modify: `Plocica/Pages/Admin/Index.cshtml`
- Modify: `Plocica/wwwroot/css/admin.css`

**Interfaces:**
- Consumes: `_AdminIcon` partial (Task 1).

- [ ] **Step 1: Add icons to the dashboard cards**

In `Plocica/Pages/Admin/Index.cshtml`, add an icon above each card's count, e.g.:

```cshtml
<a class="admin-dash-card" asp-page="/Admin/Kolekcija/Index">
    <partial name="_AdminIcon" model="@("kolekcija")" />
    <span class="admin-dash-count">@Model.ShapeCount</span>
    <span class="admin-dash-label">Kolekcija</span>
    <span class="admin-dash-link">Uredi →</span>
</a>
```

(same for Boje/`"boje"` and Projekti/`"projekti"`).

- [ ] **Step 2: Size the dashboard icon**

Append to `Plocica/wwwroot/css/admin.css`:

```css
.admin-dash-card .admin-icon { width: 1.75em; height: 1.75em; color: var(--ink-soft); }
```

- [ ] **Step 3: Add an icon to the login submit button**

In `Plocica/Pages/Admin/Login.cshtml`, replace the submit button with:

```cshtml
<button type="submit" class="icon-btn icon-btn-primary">
    <partial name="_AdminIcon" model="@("save")" />
    Prijavi se
</button>
```

- [ ] **Step 4: Build and manually verify**

Run: `dotnet build Plocica/Plocica.csproj`
Open `/Admin/Login` (log out first if needed) and `/Admin/Index` (the dashboard).
Expected: each dashboard card shows a small icon above its count; the login button shows a small icon next to its label; login still works.

- [ ] **Step 5: Commit**

```bash
git add Plocica/Pages/Admin/Login.cshtml Plocica/Pages/Admin/Index.cshtml Plocica/wwwroot/css/admin.css
git commit -m "style: add icons to dashboard cards and login button"
```

---

### Task 7: Boje — AJAX page handlers, delete the old Edit page

**Files:**
- Modify: `Plocica/Pages/Admin/Boje/Index.cshtml.cs`
- Delete: `Plocica/Pages/Admin/Boje/Edit.cshtml`
- Delete: `Plocica/Pages/Admin/Boje/Edit.cshtml.cs`

**Interfaces:**
- Produces (page handlers, all under `/Admin/Boje?handler=<Name>`):
  - `OnPostUpdateColorAsync(int id, string? hex, string? code)` → `{ ok, id, code, hex, imageUrl }` or `{ ok: false, error }` (400).
  - `OnPostAddColorAsync()` → `{ ok, id, code, hex, imageUrl }`.
  - `OnPostUpdateColorImageAsync(int id, IFormFile? file)` → `{ ok, id, code, hex, imageUrl }` or `{ ok: false, error }` (400).
  - `OnPostRemoveColorImageAsync(int id)` → `{ ok, id, code, hex, imageUrl }`.
  - `OnPostDeleteColorAsync(int id)` → `{ ok, id }`.
  - All return 404 (`NotFound()`) if `id` doesn't match an existing color.

- [ ] **Step 1: Rewrite `Index.cshtml.cs`**

```csharp
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Boje;

public class IndexModel : PageModel
{
    private static readonly Regex HexPattern = new(@"^#[0-9A-Fa-f]{6}$");

    private readonly AppDbContext _db;
    private readonly IBlobStorageService _blob;

    public IndexModel(AppDbContext db, IBlobStorageService blob)
    {
        _db = db;
        _blob = blob;
    }

    public List<ColorItem> Colors { get; set; } = new();

    public void OnGet()
    {
        Colors = _db.Colors.OrderBy(c => c.SortOrder).ToList();
    }

    public async Task<IActionResult> OnPostUpdateColorAsync(int id, string? hex, string? code)
    {
        var color = await _db.Colors.FindAsync(id);
        if (color is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { ok = false, error = "Unesite kod." });
        }

        if (hex is null || !HexPattern.IsMatch(hex))
        {
            return BadRequest(new { ok = false, error = "Neispravna boja." });
        }

        color.Code = code;
        color.Hex = hex;
        await _db.SaveChangesAsync();

        return new JsonResult(new { ok = true, id = color.Id, code = color.Code, hex = color.Hex, imageUrl = color.ImageUrl });
    }

    public async Task<IActionResult> OnPostAddColorAsync()
    {
        var nextNumber = _db.Colors.AsEnumerable()
            .Select(c => int.TryParse(c.Code, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var nextSortOrder = (_db.Colors.Select(c => (int?)c.SortOrder).Max() ?? 0) + 1;

        var color = new ColorItem
        {
            Code = nextNumber.ToString("D3"),
            Hex = "#7C8A5B",
            SortOrder = nextSortOrder,
        };

        _db.Colors.Add(color);
        await _db.SaveChangesAsync();

        return new JsonResult(new { ok = true, id = color.Id, code = color.Code, hex = color.Hex, imageUrl = color.ImageUrl });
    }

    public async Task<IActionResult> OnPostUpdateColorImageAsync(int id, IFormFile? file)
    {
        var color = await _db.Colors.FindAsync(id);
        if (color is null)
        {
            return NotFound();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { ok = false, error = "Odaberite fotografiju." });
        }

        string newImageUrl;
        try
        {
            newImageUrl = await _blob.UploadAsync(file);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ok = false, error = ex.Message });
        }

        if (!string.IsNullOrEmpty(color.ImageUrl))
        {
            await _blob.DeleteAsync(color.ImageUrl);
        }

        color.ImageUrl = newImageUrl;
        await _db.SaveChangesAsync();

        return new JsonResult(new { ok = true, id = color.Id, code = color.Code, hex = color.Hex, imageUrl = color.ImageUrl });
    }

    public async Task<IActionResult> OnPostRemoveColorImageAsync(int id)
    {
        var color = await _db.Colors.FindAsync(id);
        if (color is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(color.ImageUrl))
        {
            await _blob.DeleteAsync(color.ImageUrl);
        }

        color.ImageUrl = null;
        await _db.SaveChangesAsync();

        return new JsonResult(new { ok = true, id = color.Id, code = color.Code, hex = color.Hex, imageUrl = color.ImageUrl });
    }

    public async Task<IActionResult> OnPostDeleteColorAsync(int id)
    {
        var color = await _db.Colors.FindAsync(id);
        if (color is null)
        {
            return NotFound();
        }

        await _blob.DeleteAsync(color.ImageUrl);
        _db.Colors.Remove(color);
        await _db.SaveChangesAsync();

        return new JsonResult(new { ok = true, id });
    }
}
```

- [ ] **Step 2: Delete the old Edit page**

```bash
git rm Plocica/Pages/Admin/Boje/Edit.cshtml Plocica/Pages/Admin/Boje/Edit.cshtml.cs
```

- [ ] **Step 3: Build**

Run: `dotnet build Plocica/Plocica.csproj`
Expected: builds cleanly. `Index.cshtml` still references the old list markup at this point (rewritten in Task 8) — that's fine, `Colors` is still populated the same way by `OnGet`.

- [ ] **Step 4: Commit**

```bash
git add Plocica/Pages/Admin/Boje/Index.cshtml.cs
git commit -m "feat: add AJAX handlers for direct-edit Boje grid, remove Boje Edit page"
```

---

### Task 8: Boje — direct-edit grid frontend

**Files:**
- Modify: `Plocica/Pages/Admin/Boje/Index.cshtml`
- Create: `Plocica/wwwroot/js/color-grid.js`
- Modify: `Plocica/wwwroot/css/admin.css`

**Interfaces:**
- Consumes: page handlers from Task 7; `_AdminIcon` partial (Task 1).

- [ ] **Step 1: Rewrite `Index.cshtml`**

```cshtml
@page
@model Plocica.Pages.Admin.Boje.IndexModel
@{
    ViewData["Title"] = "Boje";
}

<div class="admin-container">
    <h1>Boje</h1>
    <p class="admin-hint">Klikni ćeliju za promjenu boje. Klikni + za dodavanje nove.</p>

    @Html.AntiForgeryToken()

    <div class="admin-color-grid"
         id="color-grid"
         data-update-url="@Url.Page("/Admin/Boje/Index", "UpdateColor")"
         data-update-image-url="@Url.Page("/Admin/Boje/Index", "UpdateColorImage")"
         data-remove-image-url="@Url.Page("/Admin/Boje/Index", "RemoveColorImage")"
         data-add-url="@Url.Page("/Admin/Boje/Index", "AddColor")"
         data-delete-url="@Url.Page("/Admin/Boje/Index", "DeleteColor")">
        @foreach (var color in Model.Colors)
        {
            <div class="admin-color-chip" data-color-id="@color.Id" data-hex="@color.Hex" data-code="@color.Code" data-image-url="@color.ImageUrl">
                <button type="button" class="admin-color-delete" data-role="delete" title="Obriši">
                    <partial name="_AdminIcon" model="@("cancel")" />
                </button>
                <div class="admin-color-swatch" data-role="swatch"
                     style="@(!string.IsNullOrEmpty(color.ImageUrl) ? $"background-image:url('{color.ImageUrl}')" : $"background:{color.Hex}")"></div>
                <span class="admin-color-code" data-role="code-label">@color.Code</span>
            </div>
        }
        <button type="button" class="admin-color-add" data-role="add" title="Dodaj boju">
            <partial name="_AdminIcon" model="@("plus")" />
        </button>
    </div>

    <div class="admin-color-popover" id="color-popover" hidden>
        <label class="admin-hint" for="color-popover-hex">Boja</label>
        <input type="color" id="color-popover-hex" />

        <label class="admin-hint" for="color-popover-code">Kod</label>
        <input type="text" id="color-popover-code" maxlength="10" />

        <div class="admin-color-popover-photo">
            <label class="icon-btn">
                <partial name="_AdminIcon" model="@("camera")" />
                Fotografija
                <input type="file" id="color-popover-photo-input" accept=".jpg,.jpeg,.png,.webp" hidden />
            </label>
            <button type="button" class="icon-btn" id="color-popover-remove-photo-btn" hidden>Ukloni fotografiju</button>
        </div>

        <p class="admin-field-error" id="color-popover-error"></p>
    </div>
</div>

@section Scripts {
    <script src="~/js/color-grid.js" asp-append-version="true"></script>
}
```

- [ ] **Step 2: Write `color-grid.js`**

```js
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
    swatch.style.backgroundImage = data.imageUrl ? "url('" + data.imageUrl + "')" : "";
    swatch.style.background = data.imageUrl ? "" : data.hex;
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
    var id = activeChip.getAttribute("data-color-id");
    try {
      var data = await postForm(urlFor("update"), { id: id, hex: hexInput.value, code: codeInput.value });
      renderChip(activeChip, data);
      showError("");
    } catch (err) {
      hexInput.value = activeChip.getAttribute("data-hex");
      codeInput.value = activeChip.getAttribute("data-code");
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
    var id = activeChip.getAttribute("data-color-id");
    var body = new FormData();
    body.append("__RequestVerificationToken", token);
    body.append("id", id);
    body.append("file", photoInput.files[0]);
    try {
      var res = await fetch(urlFor("update-image"), { method: "POST", body: body });
      var data = await res.json();
      if (!res.ok || !data.ok) throw new Error((data && data.error) || "Greška prilikom uploada.");
      renderChip(activeChip, data);
      removePhotoBtn.hidden = false;
      showError("");
    } catch (err) {
      showError(err.message);
    }
    photoInput.value = "";
  });

  removePhotoBtn.addEventListener("click", async function () {
    if (!activeChip) return;
    var id = activeChip.getAttribute("data-color-id");
    try {
      var data = await postForm(urlFor("remove-image"), { id: id });
      renderChip(activeChip, data);
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
```

- [ ] **Step 3: Add color-grid CSS**

Append to `Plocica/wwwroot/css/admin.css`:

```css
/* ---------- Boje — direct-edit grid ---------- */

.admin-color-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
  max-width: 640px;
  margin-top: 1.5rem;
}

@media (min-width: 560px) {
  .admin-color-grid { grid-template-columns: repeat(4, 1fr); }
}

@media (min-width: 860px) {
  .admin-color-grid { grid-template-columns: repeat(7, 1fr); }
}

.admin-color-chip {
  aspect-ratio: 1;
  background: var(--paper);
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  padding: 0.6em;
  position: relative;
  cursor: pointer;
  border: none;
}

.admin-color-swatch {
  flex: 1;
  margin-bottom: 0.5em;
  border-radius: 3px;
  background-size: cover;
  background-position: center;
}

.admin-color-code {
  font-variant-numeric: tabular-nums;
  font-size: 0.75rem;
  color: var(--ink-soft);
}

.admin-color-delete {
  position: absolute;
  top: 6px;
  right: 6px;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: none;
  background: var(--ink);
  color: var(--paper);
  display: none;
  align-items: center;
  justify-content: center;
  padding: 0;
  cursor: pointer;
}

.admin-color-delete .admin-icon { width: 0.7em; height: 0.7em; }

.admin-color-chip:hover .admin-color-delete,
.admin-color-chip:focus-within .admin-color-delete { display: flex; }

.admin-color-add {
  aspect-ratio: 1;
  background: var(--paper);
  border: 1px dashed var(--line);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--ink-soft);
  cursor: pointer;
}

.admin-color-add:hover { background: var(--paper-2); color: var(--ink); }
.admin-color-add .admin-icon { width: 1.4em; height: 1.4em; }

.admin-color-popover {
  position: fixed;
  background: var(--paper);
  border: 1px solid var(--line);
  border-radius: 8px;
  padding: 1em;
  width: 220px;
  box-shadow: 0 8px 20px rgba(26, 26, 23, 0.18);
  z-index: 50;
  display: flex;
  flex-direction: column;
  gap: 0.4em;
}

.admin-color-popover input[type="color"] { width: 100%; height: 40px; padding: 2px; }
.admin-color-popover input[type="text"] { width: 100%; }

.admin-color-popover-photo {
  display: flex;
  align-items: center;
  gap: 0.5em;
  margin-top: 0.4em;
  flex-wrap: wrap;
}
```

- [ ] **Step 4: Build and manually verify the full Boje flow**

Run: `dotnet build Plocica/Plocica.csproj`
Then run the app, open `/Admin/Boje`, and check:
1. The grid renders as a 7-column (on a wide window) grid of color chips matching the public `/kolekcije#boje` grid's look.
2. Click a chip → popover opens next to it with the current hex/code.
3. Change the hex via the native picker → chip swatch updates immediately, no page reload.
4. Change the code and click elsewhere → chip's code label updates.
5. Click the camera icon, choose a photo → chip swatch becomes that photo; "Ukloni fotografiju" appears; clicking it reverts the chip to its hex color.
6. Click the last cell's "+" → a new chip appears immediately and its popover opens.
7. Hover a chip → a small delete badge appears top-right; click it → native confirm dialog → confirming removes the chip.
8. Open browser devtools, switch to offline, try changing a color → the chip's swatch/code revert to their prior value and an inline error message appears near the grid (not a silent no-op).
9. Tab to a chip with the keyboard and press Enter/Space — confirm the delete badge is reachable via `:focus-within` (visible without a mouse).

- [ ] **Step 5: Commit**

```bash
git add Plocica/Pages/Admin/Boje/Index.cshtml Plocica/wwwroot/js/color-grid.js Plocica/wwwroot/css/admin.css
git commit -m "feat: replace Boje list+edit pages with direct-edit color grid"
```

---

## Self-Review

**Spec coverage:**
- Shared icon partial → Task 1.
- Sidebar nav + active state, dashboard-only brand link → Task 2.
- Shared action-button partial (icon Edit + danger Delete) → Task 3.
- Shared image-upload dropzone + live preview → Task 4.
- Shared existing-image row partial → Task 5.
- Kolekcija/Projekti per-page changes → Tasks 3, 4, 5.
- Dashboard/Login polish → Task 6.
- Boje direct-edit grid (AJAX handlers + frontend + hex/photo popover + add/delete) → Tasks 7, 8.
- Out-of-scope items (no drag-reorder, no custom modal, no public-site changes, no `Color.Name` DB removal) — none of the tasks above touch those areas.
- Testing section's manual checks are folded into each task's "manually verify" step, plus Task 8 Step 4 covers the full Boje flow including the network-failure/keyboard checks called out in the spec.

**Placeholder scan:** no TBD/TODO; every step has literal code. `ExistingExampleInput`'s actual shape (no `SortOrder`) was verified directly against `Edit.cshtml.cs` during planning, so Task 5 Step 4 wires `ShowSortOrder = false` unconditionally rather than leaving a runtime check.

**Type consistency:** `AdminActionButtonsViewModel`, `AdminImageUploadViewModel`, and `AdminImageRowViewModel` are each defined once (Tasks 3–5) and referenced with matching property names in every later call site. Page-handler names (`UpdateColor`, `AddColor`, `UpdateColorImage`, `RemoveColorImage`, `DeleteColor`) match between Task 7 (C# `OnPost<Name>Async`) and Task 8 (`Url.Page(..., "<Name>")` and the JS `data-*-url` attributes/`urlFor()` calls).
