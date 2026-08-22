# Admin panel: "Oblici" → "Kolekcija" restructure

Date: 2026-08-22

## Context

The admin panel has a single "Oblici" section backed by the `Shape` model
(`Collection` = `"oblici" | "oslikane" | "reljefne"`), all sharing one
Edit form. The public `Kolekcije` page already renders three sections
(Oblici, Ručno oslikane, Reljefne), but Reljefne's two items ("Reljefne iz
kalupa", "Ručno rezbarene") only ever show text (name/other info/price/
colors) — the admin form still exposes shape/photo/layout/example fields
for them, none of which are used.

Goal: rename the admin section to "Kolekcija", split its fields per
category so each category only exposes what it actually uses, and let the
two Reljefne items carry a plain photo gallery. Along the way, normalize
button styling and spacing across the whole admin panel.

## Data model

- `Shape.Finish` — removed (dead field once oblici/oslikane drop
  "Završna obrada"; already unused by Reljefne).
- `Shape.AvailableColors` — kept on the model (still used by Reljefne's
  text block) but no longer collected on the Oblici/Oslikane form.
- `Shape.LayoutScheme` (string) — removed, replaced by a gallery (see
  below).
- New table `ShapeGalleryImage`:
  - `Id`, `ShapeId` (FK → Shape), `Kind` (string: `"layout"` |
    `"reljefna"`), `ImageUrl`, `SortOrder`.
  - `Kind = "layout"` → shema slaganja gallery, used by Oblici/Oslikane.
  - `Kind = "reljefna"` → photo gallery, used by the two Reljefne items.
  - No name/caption field on these images (plain photos only).
- One EF Core migration covers all three changes (drop `Finish`, drop
  `LayoutScheme`, add `ShapeGalleryImages`).

## Admin panel — structure

- `Pages/Admin/Oblici/` → renamed to `Pages/Admin/Kolekcija/`
  (namespace, routes, page classes renamed to match).
- Dashboard (`Admin/Index.cshtml`): "Oblici" card/label → "Kolekcija".
- Kolekcija Index page: list grouped into three labeled sections —
  **Oblici**, **Ručno oslikane**, **Reljefne** — instead of one flat
  table with a raw collection column. Each row keeps existing
  thumb/name/sort/edit/delete actions.
- Edit form fields are conditional on the selected category, toggled
  client-side (no page reload) when the Kolekcija `<select>` changes:
  - **Oblici / Ručno oslikane**: Naziv, Skica (ImageUrl), Debljina,
    Dimenzije, Shema slaganja (gallery, `Kind="layout"`), Ostali info,
    Opća slika (PhotoUrl), Cijena, Primjeri (unchanged), Redoslijed.
  - **Reljefne**: Naziv, Ostali info, Cijena, Dostupne boje, Fotografije
    (gallery, `Kind="reljefna"`), Redoslijed.
  - "Završna obrada" field removed everywhere.
- The existing dynamic add/remove-row JS (currently hardcoded for
  "Primjeri" in `admin.js`) is generalized into one reusable helper
  driven by data attributes, so it drives all three dynamic image lists
  (Primjeri, Shema slaganja, Reljefne fotografije) without three copies
  of the same logic.

## Admin panel — visual consistency

- Ditch `.btn` / `.btn-cta` (the clip-path/gradient "stylised" button)
  everywhere in the admin panel — that treatment stays exclusive to the
  public site.
- Add plain admin-native button classes in `admin.css` (e.g.
  `.admin-btn`, `.admin-btn-primary`): simple bordered rectangle, no
  clip-path, no gradient overlay, hover/active states consistent with
  existing admin chrome (`.admin-logout-btn`, `.admin-delete-btn`).
- Replace every `btn`/`btn-cta` usage across the admin panel (Login,
  Boje, Projekti, Kolekcija index/edit, "+ Dodaj" buttons) with the new
  classes, so all admin actions look and space consistently regardless
  of which page they're on.

## Public site

- `_ShapeGrid.cshtml` (Oblici/Oslikane detail box): "Shema slaganja" row
  becomes a small image row (renders the `Kind="layout"` gallery)
  instead of text; "Završna obrada" row removed entirely.
- `Kolekcije.cshtml` Reljefne section: keeps its current text spec-row
  per item (name/other info/price/colors) and gets a simple responsive
  thumbnail row appended per item, rendering that item's `Kind="reljefna"`
  gallery. No lightbox/carousel — plain image grid.

## Out of scope

- No lightbox/carousel/reorder-by-drag UI (SortOrder input is enough).
- No renaming of the two Reljefne categories in the UI beyond editing
  the existing item's Naziv field (already supported).
- No changes to Boje/Projekti data models — only their button styling
  is touched, for consistency.

## Testing

- Manual: create/edit an Oblici and an Oslikane shape, upload a shema
  slaganja gallery and primjeri, confirm they render on `/kolekcije`.
- Manual: edit a Reljefne item, upload gallery photos, confirm they
  render under the existing text spec-row on `/kolekcije`.
- Manual: verify admin button styling is visually consistent across
  Login, Boje, Projekti, and Kolekcija pages, and no page shows the
  clip-path CTA button.
- `dotnet build` to confirm the migration and model changes compile and
  apply cleanly against a fresh dev DB.
