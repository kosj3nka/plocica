# Admin panel UI/UX redesign: sidebar nav, icons, previews, direct-edit Boje

Date: 2026-08-22

## Context

The admin panel (`Pages/Admin/**`, layout `Pages/Shared/_AdminLayout.cshtml`)
was recently restructured (Oblici → Kolekcija rename, per-category Edit
fields, normalized `.admin-btn` button styling — see
`2026-08-22-admin-kolekcija-restructure-design.md`). That work fixed button
*styling* consistency but left the panel's structure and interaction model
untouched:

- Top nav is 3 plain text links, no active-page indicator.
- No icon anywhere in the admin UI — every action is a text link/button
  ("Uredi", "Obriši", "Spremi", "Odustani", "Novi").
- Delete is a bare underlined text link (`.admin-delete-btn`), not a real
  button.
- Image upload fields show a static preview of the *existing* image only;
  a newly selected file has no preview before saving.
- Kolekcija/Edit, Projekti/Edit, Boje/Edit each hand-roll a near-identical
  "existing image row: thumb + sort input + delete checkbox" block.
- Boje is a conventional list+edit CRUD pair, disconnected from how colors
  actually render on the public site (`Kolekcije.cshtml`'s `.color-grid`).

Goal: make the whole admin panel coherent, icon-literate, and faster to use
— a left sidebar with active-state + icons, a shared icon set, live image
previews, deduplicated shared partials for repeated patterns, and a
from-scratch direct-manipulation grid for Boje that mirrors the public
color grid.

## Shared components (new)

- **`Pages/Shared/_AdminIcons.cshtml`** — a Razor helper (`@await
  Html.PartialAsync` per named icon, or a small `IHtmlHelper` extension)
  exposing inline `<svg>` icons: edit (pencil), delete (trash), save
  (disk), cancel, plus (add), upload/image, dashboard, kolekcija, boje,
  projekti, logout, external-link, camera (photo-override). Every icon
  uses `fill="none" stroke="currentColor"` so it inherits the button's
  text color — no new asset files, no icon font, consistent with the
  existing hand-inlined footer SVGs.
- **`Pages/Shared/_AdminNav.cshtml`** — the sidebar, included from
  `_AdminLayout.cshtml`. Dark (`--ink`) background, brand text "Pločice
  Admin" at top linking to `Admin/Index` (dashboard — no longer a separate
  nav row), three icon+label links (Kolekcija/Boje/Projekti), logout
  pinned at the bottom. Active state: compare
  `ViewContext.RouteData.Values["page"]` (or the current `PageContext`)
  against each link's target and apply an `.active` class — a gold
  (`--glz-sun`) left border + full-opacity text, mirroring the mockup.
- **`_AdminActionButtons.cshtml`** partial — takes an edit URL and a
  delete-form (page handler + route id + confirm text), renders the
  icon+label Edit button and a real danger-styled Delete button
  (`--glz-clay` border/text) instead of the current bare text link. Used
  by Kolekcija/Boje-legacy tables (n/a after Boje restructure, see below)
  and Projekti's list rows.
- **`_AdminImageUpload.cshtml`** partial + **`wwwroot/js/admin-upload.js`**
  — a dropzone (`<label>` wrapping a styled drop area + hidden `<input
  type=file>`), showing the *existing* image if present, and on file
  selection swapping in a live preview via
  `URL.createObjectURL(input.files[0])` assigned to an `<img>`. Supports
  drag-over styling and click-to-browse. Degrades to a plain file input
  if JS fails to load (input is real, preview is progressive enhancement
  only).
- **`_AdminImageRowList.cshtml`** partial — the repeatable "thumbnail +
  filename + order input + remove icon-button" row block, replacing the
  duplicated markup in Kolekcija/Edit (Primjeri, Shema slaganja, Reljefne
  fotografije — already generalized into one JS-driven add/remove helper
  per the prior restructure spec) and Projekti/Edit's gallery.

## Sidebar navigation

Replaces `_AdminLayout.cshtml`'s current flat top-bar nav. Layout becomes a
flex row: fixed-width (`220px`) sidebar + flexible content area. Sidebar
content:

1. Brand/logo, links to `Admin/Index`.
2. Kolekcija / Boje / Projekti — icon + label, active one gold-barred.
3. Spacer.
4. Logout — icon + label, submits the existing `Admin/Logout` POST form.

`Admin/Index` (dashboard) keeps its 3 stat cards, restyled with icons per
card, reachable only via the brand link now (matches the earlier chat
approval — no dedicated "Nadzorna ploča" nav row).

Small screens: below `860px` the sidebar collapses to a slim icon-only rail
(labels hidden, `220px` → `64px`) rather than an overlay/hamburger — admin
usage is overwhelmingly desktop, but a middling-width case (tablet) should
stay usable without extra JS.

## Per-page changes

- **Kolekcija** (`Admin/Kolekcija/Index.cshtml`, `Edit.cshtml`): list page
  keeps its three stacked labeled tables (Oblici / Ručno oslikane /
  Reljefne per the prior restructure), restyled to use
  `_AdminActionButtons`. Edit page's image fields (Skica, Opća slika,
  Primjeri, Shema slaganja, Reljefne fotografije) switch to
  `_AdminImageUpload` / `_AdminImageRowList`. No field/model changes.
- **Boje** — replaces `Admin/Boje/Index.cshtml` + `Edit.cshtml` with a
  single `Admin/Boje/Index.cshtml` rendering a direct-edit grid:
  - Grid CSS mirrors the public `.color-grid`/`.color-chip` exactly (1px
    gap on `--line`, `aspect-ratio: 1`, responsive `repeat(3/4/7, 1fr)`
    breakpoints at `560px`/`860px`), so what admins see is what the public
    page will show.
  - Each chip: swatch (background = `Hex`, or `background-image` when an
    `ImageUrl` override is set) + `Code` below it. `Name` is dropped from
    the admin UI (field stays on the model/migration-wise untouched but is
    no longer editable or shown here — it was already optional/unused
    visually per the public grid's tiny gray label).
  - Click a chip → an inline popover (absolutely positioned off the chip)
    with a native `<input type="color">` bound to `Hex`, a small text
    input for `Code`, and a camera-icon secondary button that reveals a
    file input to set `ImageUrl` instead (clears `Hex` display in favor of
    the photo once set; clearing the photo falls back to `Hex`).
  - Hovering a chip reveals a small `×` badge (top-right) that deletes the
    chip; kept behind the existing `admin.js` `confirm()` pattern
    (`data-confirm="Obrisati boju {code}?"`).
  - Last cell is always a dashed `+` chip that appends a new color
    (server assigns the next `Code`/`Order`, client just calls "add").
  - **New AJAX endpoints** on `Admin/Boje/Index.cshtml.cs`:
    `OnPostUpdateColorAsync(int id, string hex, string code)`,
    `OnPostAddColorAsync()`, `OnPostUpdateColorImageAsync(int id, IFormFile
    file)`, `OnPostDeleteColorAsync(int id)` — each returns a small JSON
    result (`{ ok: true, color: {...} }` or `{ ok: false, error: "..." }`)
    instead of a redirect. This is the one place in the admin panel moving
    off full-page POST/redirect: a full reload on every hex tweak or
    add/delete would be a bad fit for a grid meant to feel like direct
    manipulation.
  - **New `wwwroot/js/color-grid.js`**: opens/closes the popover, wires
    the native color input's `change` event (not `input`, to avoid firing
    a request on every drag step) and the code text field's `blur`/Enter
    to `fetch()` the update endpoint and patch the chip's swatch/code in
    place; wires the `+` chip and the `×` badge to their endpoints the
    same way. On a non-OK response or network failure, the chip visually
    reverts (re-render from the last known-good value) and a small inline
    error message appears near the grid — no silent no-ops.
  - Order: chips render in `SortOrder`/creation order; no drag-reorder in
    this pass (matches the "Out of scope" precedent from the prior admin
    spec — SortOrder stays a plain field, just not exposed as a UI control
    here since the grid has no reorder affordance yet).
- **Projekti** (`Admin/Projekti/Index.cshtml`, `Edit.cshtml`): list
  restyled with `_AdminActionButtons`; edit page's gallery uses
  `_AdminImageUpload` / `_AdminImageRowList`, same as Kolekcija.
- **Login** (`Admin/Login.cshtml`): visual polish only — icon-labeled
  submit button, consistent field styling with the rest of the panel. No
  structural or auth-flow change.

## CSS/JS footprint

All additions live in `admin.css` (sidebar, `.icon-btn`/`.icon-btn.danger`/
`.icon-btn.primary`, `.dropzone`, `.admin-color-grid`/`.admin-color-chip`,
`.img-row` styles) — no CSS framework introduced, consistent with the
project's existing hand-written approach. New JS files
(`admin-upload.js`, `color-grid.js`) follow the existing `admin.js`/
`admin-toggle.js` style: small, vanilla, delegated event listeners, no
build step, no bundler.

## Data model

No schema changes except what's implied by dropping `Name` from the Boje
admin UI (field stays on `Color` — out of scope to migrate it away, it's
simply unused by the new grid) and the `ImageUrl`-as-override semantics
already existing on the model (Hex primary, ImageUrl optional override —
matches current `Color` fields, no migration needed).

## Out of scope

- Drag-and-drop reordering anywhere (Kolekcija/Projekti image rows or the
  Boje grid) — numeric `SortOrder` inputs stay for Kolekcija/Projekti;
  Boje grid has no reorder UI in this pass.
- Custom modal confirmation dialogs — native `confirm()` stays for every
  delete action, including the Boje grid's `×`.
- Any change to the public site (`Kolekcije.cshtml`, `_ShapeGrid.cshtml`)
  — this spec is admin-only. The Boje grid mirrors the public grid's CSS
  but the public page itself is untouched.
- Tabbed Kolekcija list (considered, explicitly rejected in favor of
  keeping the 3 stacked tables, restyled).
- Removing the `Color.Name` field from the database — only its admin UI
  exposure is dropped.

## Testing

- Manual: create/edit/delete a Kolekcija shape and a Projekti project;
  confirm the new dropzone preview shows the newly selected file before
  save, and image-row remove/reorder still work.
- Manual: full Boje grid flow — add a chip via `+`, set its color via the
  native picker, set a code, upload a photo override via the camera
  button, delete a chip via hover `×` + confirm, and confirm a simulated
  network failure (e.g. throttle/offline in devtools) reverts the chip
  and shows the inline error instead of silently doing nothing.
- Manual: keyboard-only pass — tab through the sidebar, open a Boje
  popover with keyboard, confirm focus-visible outlines appear (existing
  `:focus-visible` quality-floor rule in `tokens.css` should already
  cover new interactive elements since they reuse `button`/`input`/`a`).
- Manual: resize to a tablet width (~800px) and confirm the sidebar
  collapses to the icon-only rail without breaking layout.
- `dotnet build` to confirm the new Boje page-handler signatures compile.
