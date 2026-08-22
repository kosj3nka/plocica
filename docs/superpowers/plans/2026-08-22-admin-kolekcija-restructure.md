# Admin Kolekcija Restructure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rename the admin "Oblici" section to "Kolekcija" with three categories (Oblici, Ručno oslikane sharing one field set; Reljefne with its own smaller field set + photo gallery), replace the "shema slaganja" text field with an image gallery, and make admin buttons plain everywhere in the admin panel (the clip-path "stylised" button stays exclusive to the public site).

**Architecture:** `Shape` stays the single backing entity for all three categories (as today), but loses `Finish` (dropped entirely) and `LayoutScheme` (replaced by a gallery). A new `ShapeGalleryImage` join table (`Kind = "layout" | "reljefna"`) backs both the new "shema slaganja" gallery (Oblici/Oslikane) and the new photo gallery on the two existing Reljefne items — one table, one CRUD code path, instead of two near-identical ones. The two galleries reuse the existing "multi-file upload + existing-image rows with sort/delete" pattern already used by `ProjectImage` (`Pages/Admin/Projekti/Edit.cshtml.cs`), not the named-example JS pattern — it needs no new JavaScript. The admin category picker becomes a radio group reusing the existing `data-toggle-target` / `.admin-toggle-panel` show/hide mechanism already used by `Pages/Admin/Boje/Edit.cshtml` (its toggle script gets renamed from `admin-color-toggle.js` to the more accurate `admin-toggle.js` since it's now used in two places).

**Tech Stack:** ASP.NET Core 8 Razor Pages, EF Core 8.0.11 (SQL Server), Azure Blob Storage via `IBlobStorageService`. No automated test project exists in this repo — verification is `dotnet build` plus manual browser checks, as already practiced here.

**Spec:** [docs/superpowers/specs/2026-08-22-admin-kolekcija-restructure-design.md](../specs/2026-08-22-admin-kolekcija-restructure-design.md)

## Global Constraints

- No automated test suite exists in this repo (confirmed: no `*Test*` project). Every task's checkpoint is `dotnet build` (must show 0 errors once the task's files are self-consistent) plus, where noted, a manual check by running the app.
- Blob upload rules are already enforced by `IBlobStorageService.UploadAsync` (JPG/PNG/WEBP, ≤5 MB) — do not duplicate that validation in Razor Pages code.
- All UI copy is Croatian, matching existing tone (see any existing `.cshtml` under `Pages/Admin` for examples).
- `ImplicitUsings` is enabled (`Plocica.csproj`) — do not add redundant `using System;` / `using System.Linq;` etc.
- Keep the existing file-per-page Razor Pages convention (`Index.cshtml`/`Index.cshtml.cs`, `Edit.cshtml`/`Edit.cshtml.cs`) — do not introduce partials/components unless a task explicitly says to.
- Admin buttons must never use `.btn` / `.btn-cta` (tokens.css) after this plan — those classes stay exclusive to the public site.

---

## Task 1: Data model — drop `Finish`/`LayoutScheme`, add `ShapeGalleryImage`

**Files:**
- Modify: `Plocica/Models/Shape.cs`
- Create: `Plocica/Models/ShapeGalleryImage.cs`
- Modify: `Plocica/Data/AppDbContext.cs`
- Modify: `Plocica/Data/DbInitializer.cs`

**Interfaces:**
- Produces: `Plocica.Models.ShapeGalleryImage { Id, ShapeId, Shape, Kind, ImageUrl, SortOrder }` with constants `ShapeGalleryImage.KindLayout = "layout"` and `ShapeGalleryImage.KindReljefna = "reljefna"`; `Shape.GalleryImages` (`List<ShapeGalleryImage>`); `AppDbContext.ShapeGalleryImages` (`DbSet<ShapeGalleryImage>`). Tasks 2 and 3 consume these exact names.

- [ ] **Step 1: Rewrite `Shape.cs`** — remove `Finish` and `LayoutScheme`, add the `GalleryImages` navigation:

```csharp
namespace Plocica.Models;

public class Shape
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;      // "Arabesque"
    public string Collection { get; set; } = string.Empty; // "oblici" | "oslikane" | "reljefne"
    public string? Thickness { get; set; }                 // debljina, npr. "0,8 cm"
    public string? Dimensions { get; set; }                 // dimenzija, npr. "14,5 × 14,5 cm"
    public string? ImageUrl { get; set; }                    // individualna slika (Blob URL) — ikona u mreži oblika
    public string? PhotoUrl { get; set; }                     // opća fotografija (Blob URL) — prikaz u tehničkim informacijama
    public string? AvailableColors { get; set; }               // dostupne boje — koristi samo kategorija "reljefne"
    public string? OtherInfo { get; set; }                       // ostali info
    public string? Price { get; set; }                            // cijena — tekst, može biti višeredna
    public int SortOrder { get; set; }
    public List<ShapeExample> Examples { get; set; } = new();   // primjeri: naziv + fotografija (npr. "Blue")
    public List<ShapeGalleryImage> GalleryImages { get; set; } = new(); // shema slaganja (Kind=Layout) i reljefne fotografije (Kind=Reljefna)
}
```

- [ ] **Step 2: Create `Plocica/Models/ShapeGalleryImage.cs`:**

```csharp
namespace Plocica.Models;

public class ShapeGalleryImage
{
    public const string KindLayout = "layout";
    public const string KindReljefna = "reljefna";

    public int Id { get; set; }
    public int ShapeId { get; set; }
    public Shape Shape { get; set; } = null!;
    public string Kind { get; set; } = string.Empty;      // "layout" | "reljefna"
    public string ImageUrl { get; set; } = string.Empty;  // Blob URL
    public int SortOrder { get; set; }
}
```

- [ ] **Step 3: Add the `DbSet` in `Plocica/Data/AppDbContext.cs`** (insert after the `ShapeExamples` line):

```csharp
    public DbSet<Shape> Shapes => Set<Shape>();
    public DbSet<ShapeExample> ShapeExamples => Set<ShapeExample>();
    public DbSet<ShapeGalleryImage> ShapeGalleryImages => Set<ShapeGalleryImage>();
    public DbSet<ColorItem> Colors => Set<ColorItem>();
```

- [ ] **Step 4: Remove `Finish` from `Plocica/Data/DbInitializer.cs`** — delete the line `const string finish = "mat i sjajno";` and, using a single find-and-replace-all across the file, delete every occurrence of the line:

```csharp
                Finish = finish,
```

(It appears 7 times, once per Oblici/Oslikane seed entry — Arabesque, Oval, Fish scale, Linea, Wave, Module, Curve. The two Reljefne entries never set `Finish`, so nothing to change there.)

- [ ] **Step 5: Build and confirm the expected (partial) failure**

Run: `dotnet build` (from `c:\Users\korisnik\Gita\pločice\Plocica`)
Expected: FAIL. The only errors should be in `Pages/Admin/Oblici/Edit.cshtml.cs`, `Pages/Admin/Oblici/Edit.cshtml`, and `Pages/Shared/_ShapeGrid.cshtml` (`Finish`/`LayoutScheme` no longer exist on `Shape`). Confirm no *other* files show errors — that would mean something in this task's edits is wrong. Task 2 and Task 3 fix these remaining errors.

- [ ] **Step 6: Commit**

```bash
git add Plocica/Models/Shape.cs Plocica/Models/ShapeGalleryImage.cs Plocica/Data/AppDbContext.cs Plocica/Data/DbInitializer.cs
git commit -m "feat: replace Shape.Finish/LayoutScheme with ShapeGalleryImage gallery"
```

---

## Task 2: Admin "Kolekcija" pages (rename from Oblici, category-aware Edit form, two new galleries)

**Files:**
- Create: `Plocica/Pages/Admin/Kolekcija/Index.cshtml`
- Create: `Plocica/Pages/Admin/Kolekcija/Index.cshtml.cs`
- Create: `Plocica/Pages/Admin/Kolekcija/Edit.cshtml`
- Create: `Plocica/Pages/Admin/Kolekcija/Edit.cshtml.cs`
- Delete: `Plocica/Pages/Admin/Oblici/Index.cshtml`
- Delete: `Plocica/Pages/Admin/Oblici/Index.cshtml.cs`
- Delete: `Plocica/Pages/Admin/Oblici/Edit.cshtml`
- Delete: `Plocica/Pages/Admin/Oblici/Edit.cshtml.cs`

**Interfaces:**
- Consumes: `Plocica.Models.ShapeGalleryImage` / `Shape.GalleryImages` from Task 1.
- Produces: routes `/Admin/Kolekcija/Index` and `/Admin/Kolekcija/Edit`, namespace `Plocica.Pages.Admin.Kolekcija`. Task 4 (dashboard/nav) links to these routes; Task 5 restyles the buttons already written here.

- [ ] **Step 1: Create `Plocica/Pages/Admin/Kolekcija/Index.cshtml.cs`:**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Kolekcija;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IBlobStorageService _blob;

    public IndexModel(AppDbContext db, IBlobStorageService blob)
    {
        _db = db;
        _blob = blob;
    }

    public List<Shape> ObliciShapes { get; set; } = new();
    public List<Shape> OslikaneShapes { get; set; } = new();
    public List<Shape> ReljefneShapes { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public void OnGet()
    {
        var shapes = _db.Shapes.OrderBy(s => s.SortOrder).ToList();
        ObliciShapes = shapes.Where(s => s.Collection == "oblici").ToList();
        OslikaneShapes = shapes.Where(s => s.Collection == "oslikane").ToList();
        ReljefneShapes = shapes.Where(s => s.Collection == "reljefne").ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var shape = await _db.Shapes.FindAsync(id);
        if (shape is not null)
        {
            await _blob.DeleteAsync(shape.ImageUrl);
            _db.Shapes.Remove(shape);
            await _db.SaveChangesAsync();
            Message = $"\"{shape.Name}\" je obrisano.";
        }

        return RedirectToPage("Index");
    }
}
```

- [ ] **Step 2: Create `Plocica/Pages/Admin/Kolekcija/Index.cshtml`:**

```html
@page
@model Plocica.Pages.Admin.Kolekcija.IndexModel
@{
    ViewData["Title"] = "Kolekcija";
}

<div class="admin-container">
    <div class="admin-list-header">
        <h1>Kolekcija</h1>
        <a asp-page="Edit" class="admin-btn admin-btn-primary">Novi</a>
    </div>

    @if (Model.Message is not null)
    {
        <p class="admin-success">@Model.Message</p>
    }

    <div class="admin-section">
        <p class="admin-section-label">Oblici</p>
        <div class="admin-table-wrap">
            <table class="admin-table">
                <thead>
                    <tr>
                        <th></th>
                        <th>Naziv</th>
                        <th>Redoslijed</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var shape in Model.ObliciShapes)
                    {
                        <tr>
                            <td>
                                @if (!string.IsNullOrEmpty(shape.ImageUrl))
                                {
                                    <img src="@shape.ImageUrl" alt="" class="admin-thumb" />
                                }
                                else
                                {
                                    <span class="admin-thumb admin-thumb-empty"></span>
                                }
                            </td>
                            <td>@shape.Name</td>
                            <td>@shape.SortOrder</td>
                            <td class="admin-row-actions">
                                <a asp-page="Edit" asp-route-id="@shape.Id">Uredi</a>
                                <form method="post" asp-page-handler="Delete" asp-route-id="@shape.Id" class="admin-delete-form" data-confirm="Obrisati oblik &quot;@shape.Name&quot;?">
                                    <button type="submit" class="admin-delete-btn">Obriši</button>
                                </form>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>

    <div class="admin-section">
        <p class="admin-section-label">Ručno oslikane</p>
        <div class="admin-table-wrap">
            <table class="admin-table">
                <thead>
                    <tr>
                        <th></th>
                        <th>Naziv</th>
                        <th>Redoslijed</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var shape in Model.OslikaneShapes)
                    {
                        <tr>
                            <td>
                                @if (!string.IsNullOrEmpty(shape.ImageUrl))
                                {
                                    <img src="@shape.ImageUrl" alt="" class="admin-thumb" />
                                }
                                else
                                {
                                    <span class="admin-thumb admin-thumb-empty"></span>
                                }
                            </td>
                            <td>@shape.Name</td>
                            <td>@shape.SortOrder</td>
                            <td class="admin-row-actions">
                                <a asp-page="Edit" asp-route-id="@shape.Id">Uredi</a>
                                <form method="post" asp-page-handler="Delete" asp-route-id="@shape.Id" class="admin-delete-form" data-confirm="Obrisati oblik &quot;@shape.Name&quot;?">
                                    <button type="submit" class="admin-delete-btn">Obriši</button>
                                </form>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>

    <div class="admin-section">
        <p class="admin-section-label">Reljefne</p>
        <div class="admin-table-wrap">
            <table class="admin-table">
                <thead>
                    <tr>
                        <th>Naziv</th>
                        <th>Redoslijed</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var shape in Model.ReljefneShapes)
                    {
                        <tr>
                            <td>@shape.Name</td>
                            <td>@shape.SortOrder</td>
                            <td class="admin-row-actions">
                                <a asp-page="Edit" asp-route-id="@shape.Id">Uredi</a>
                                <form method="post" asp-page-handler="Delete" asp-route-id="@shape.Id" class="admin-delete-form" data-confirm="Obrisati stavku &quot;@shape.Name&quot;?">
                                    <button type="submit" class="admin-delete-btn">Obriši</button>
                                </form>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>

@section Scripts {
    <script src="~/js/admin.js" asp-append-version="true"></script>
}
```

- [ ] **Step 3: Create `Plocica/Pages/Admin/Kolekcija/Edit.cshtml.cs`:**

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Kolekcija;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IBlobStorageService _blob;

    public EditModel(AppDbContext db, IBlobStorageService blob)
    {
        _db = db;
        _blob = blob;
    }

    public int? Id { get; set; }
    public string? ExistingImageUrl { get; set; }
    public string? ExistingPhotoUrl { get; set; }

    [BindProperty]
    public ShapeInput Input { get; set; } = new();

    public class ShapeInput
    {
        [Required(ErrorMessage = "Unesite naziv.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Odaberite kategoriju.")]
        public string Collection { get; set; } = "oblici";

        public string? Thickness { get; set; }
        public string? Dimensions { get; set; }
        public string? AvailableColors { get; set; }
        public string? OtherInfo { get; set; }
        public string? Price { get; set; }
        public int SortOrder { get; set; }
        public IFormFile? ImageFile { get; set; }
        public IFormFile? PhotoFile { get; set; }

        public List<ExistingExampleInput> ExistingExamples { get; set; } = new();
        public List<NewExampleInput> NewExamples { get; set; } = new();

        public List<ExistingGalleryImageInput> ExistingLayoutImages { get; set; } = new();
        public List<IFormFile>? NewLayoutImageFiles { get; set; }

        public List<ExistingGalleryImageInput> ExistingReljefneImages { get; set; } = new();
        public List<IFormFile>? NewReljefneImageFiles { get; set; }
    }

    public class ExistingExampleInput
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool Delete { get; set; }
    }

    public class NewExampleInput
    {
        public string? Name { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    public class ExistingGalleryImageInput
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool Delete { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Id = id;

        if (id is not null)
        {
            var shape = await _db.Shapes
                .Include(s => s.Examples)
                .Include(s => s.GalleryImages)
                .FirstOrDefaultAsync(s => s.Id == id.Value);
            if (shape is null)
            {
                return RedirectToPage("Index");
            }

            Input = new ShapeInput
            {
                Name = shape.Name,
                Collection = shape.Collection,
                Thickness = shape.Thickness,
                Dimensions = shape.Dimensions,
                AvailableColors = shape.AvailableColors,
                OtherInfo = shape.OtherInfo,
                Price = shape.Price,
                SortOrder = shape.SortOrder,
                ExistingExamples = shape.Examples
                    .OrderBy(e => e.SortOrder)
                    .Select(e => new ExistingExampleInput { Id = e.Id, Name = e.Name, ImageUrl = e.ImageUrl })
                    .ToList(),
                ExistingLayoutImages = shape.GalleryImages
                    .Where(g => g.Kind == ShapeGalleryImage.KindLayout)
                    .OrderBy(g => g.SortOrder)
                    .Select(g => new ExistingGalleryImageInput { Id = g.Id, ImageUrl = g.ImageUrl, SortOrder = g.SortOrder })
                    .ToList(),
                ExistingReljefneImages = shape.GalleryImages
                    .Where(g => g.Kind == ShapeGalleryImage.KindReljefna)
                    .OrderBy(g => g.SortOrder)
                    .Select(g => new ExistingGalleryImageInput { Id = g.Id, ImageUrl = g.ImageUrl, SortOrder = g.SortOrder })
                    .ToList(),
            };
            ExistingImageUrl = shape.ImageUrl;
            ExistingPhotoUrl = shape.PhotoUrl;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        Id = id;
        Shape? shape = null;

        if (id is not null)
        {
            shape = await _db.Shapes
                .Include(s => s.Examples)
                .Include(s => s.GalleryImages)
                .FirstOrDefaultAsync(s => s.Id == id.Value);
            if (shape is null)
            {
                return RedirectToPage("Index");
            }

            ExistingImageUrl = shape.ImageUrl;
            ExistingPhotoUrl = shape.PhotoUrl;
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var newImageUrl = ExistingImageUrl;

        if (Input.ImageFile is not null && Input.ImageFile.Length > 0)
        {
            try
            {
                newImageUrl = await _blob.UploadAsync(Input.ImageFile);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Input.ImageFile", ex.Message);
                return Page();
            }

            if (!string.IsNullOrEmpty(ExistingImageUrl))
            {
                await _blob.DeleteAsync(ExistingImageUrl);
            }
        }

        var newPhotoUrl = ExistingPhotoUrl;

        if (Input.PhotoFile is not null && Input.PhotoFile.Length > 0)
        {
            try
            {
                newPhotoUrl = await _blob.UploadAsync(Input.PhotoFile);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Input.PhotoFile", ex.Message);
                return Page();
            }

            if (!string.IsNullOrEmpty(ExistingPhotoUrl))
            {
                await _blob.DeleteAsync(ExistingPhotoUrl);
            }
        }

        if (shape is null)
        {
            shape = new Shape();
            _db.Shapes.Add(shape);
        }

        shape.Name = Input.Name;
        shape.Collection = Input.Collection;
        shape.Thickness = Input.Thickness;
        shape.Dimensions = Input.Dimensions;
        shape.AvailableColors = Input.AvailableColors;
        shape.OtherInfo = Input.OtherInfo;
        shape.Price = Input.Price;
        shape.SortOrder = Input.SortOrder;
        shape.ImageUrl = newImageUrl;
        shape.PhotoUrl = newPhotoUrl;

        // Ažuriranje / brisanje postojećih primjera.
        foreach (var existing in Input.ExistingExamples)
        {
            var example = shape.Examples.FirstOrDefault(e => e.Id == existing.Id);
            if (example is null)
            {
                continue;
            }

            if (existing.Delete)
            {
                await _blob.DeleteAsync(example.ImageUrl);
                _db.ShapeExamples.Remove(example);
            }
            else
            {
                example.Name = existing.Name;
            }
        }

        // Dodavanje novih primjera.
        var nextExampleSortOrder = shape.Examples.Any() ? shape.Examples.Max(e => e.SortOrder) + 1 : 0;

        foreach (var newExample in Input.NewExamples)
        {
            if (newExample.ImageFile is null || newExample.ImageFile.Length == 0)
            {
                continue;
            }

            string url;
            try
            {
                url = await _blob.UploadAsync(newExample.ImageFile);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            shape.Examples.Add(new ShapeExample
            {
                Name = newExample.Name ?? string.Empty,
                ImageUrl = url,
                SortOrder = nextExampleSortOrder,
            });
            nextExampleSortOrder++;
        }

        if (!await TryApplyGalleryAsync(shape, ShapeGalleryImage.KindLayout, Input.ExistingLayoutImages, Input.NewLayoutImageFiles, nameof(Input.NewLayoutImageFiles)))
        {
            return Page();
        }

        if (!await TryApplyGalleryAsync(shape, ShapeGalleryImage.KindReljefna, Input.ExistingReljefneImages, Input.NewReljefneImageFiles, nameof(Input.NewReljefneImageFiles)))
        {
            return Page();
        }

        await _db.SaveChangesAsync();

        TempData["Message"] = $"\"{shape.Name}\" je spremljeno.";
        return RedirectToPage("Index");
    }

    private async Task<bool> TryApplyGalleryAsync(
        Shape shape,
        string kind,
        List<ExistingGalleryImageInput> existingImages,
        List<IFormFile>? newFiles,
        string newFilesFieldName)
    {
        foreach (var existing in existingImages)
        {
            var image = shape.GalleryImages.FirstOrDefault(g => g.Id == existing.Id && g.Kind == kind);
            if (image is null)
            {
                continue;
            }

            if (existing.Delete)
            {
                await _blob.DeleteAsync(image.ImageUrl);
                _db.ShapeGalleryImages.Remove(image);
            }
            else
            {
                image.SortOrder = existing.SortOrder;
            }
        }

        if (newFiles is not null)
        {
            var kindImages = shape.GalleryImages.Where(g => g.Kind == kind).ToList();
            var nextSortOrder = kindImages.Any() ? kindImages.Max(g => g.SortOrder) + 1 : 0;

            foreach (var file in newFiles.Where(f => f.Length > 0))
            {
                string url;
                try
                {
                    url = await _blob.UploadAsync(file);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(newFilesFieldName, ex.Message);
                    return false;
                }

                shape.GalleryImages.Add(new ShapeGalleryImage
                {
                    Kind = kind,
                    ImageUrl = url,
                    SortOrder = nextSortOrder,
                });
                nextSortOrder++;
            }
        }

        return true;
    }
}
```

- [ ] **Step 4: Create `Plocica/Pages/Admin/Kolekcija/Edit.cshtml`:**

```html
@page "{id:int?}"
@model Plocica.Pages.Admin.Kolekcija.EditModel
@{
    ViewData["Title"] = Model.Id is null ? "Nova stavka" : "Uredi stavku";
}

<div class="admin-container">
    <a class="admin-back" asp-page="Index">← Kolekcija</a>
    <h1>@(Model.Id is null ? "Nova stavka" : "Uredi stavku")</h1>

    <form method="post" enctype="multipart/form-data" class="admin-form">
        <div asp-validation-summary="ModelOnly" class="admin-alert"></div>

        <div class="admin-field">
            <label asp-for="Input.Name">Naziv</label>
            <input asp-for="Input.Name" />
            <span asp-validation-for="Input.Name" class="admin-field-error"></span>
        </div>

        <div class="admin-field">
            <label>Kategorija</label>
            <div class="admin-toggle">
                <label><input type="radio" asp-for="Input.Collection" value="oblici" data-toggle-target="puna" /> Oblici</label>
                <label><input type="radio" asp-for="Input.Collection" value="oslikane" data-toggle-target="puna" /> Ručno oslikane</label>
                <label><input type="radio" asp-for="Input.Collection" value="reljefne" data-toggle-target="reljefne" /> Reljefne</label>
            </div>
        </div>

        <div class="admin-toggle-panel" data-panel="puna">
            <div class="admin-field-row">
                <div class="admin-field">
                    <label asp-for="Input.Thickness">Debljina</label>
                    <input asp-for="Input.Thickness" placeholder="npr. 0,8 cm" />
                </div>
                <div class="admin-field">
                    <label asp-for="Input.Dimensions">Dimenzija</label>
                    <input asp-for="Input.Dimensions" placeholder="npr. 14,5 × 14,5 cm" />
                </div>
            </div>

            <div class="admin-field">
                <label>Skica (ikona u mreži oblika)</label>
                @if (!string.IsNullOrEmpty(Model.ExistingImageUrl))
                {
                    <img src="@Model.ExistingImageUrl" alt="" class="admin-thumb-preview" />
                }
                <input asp-for="Input.ImageFile" type="file" accept=".jpg,.jpeg,.png,.webp" />
                <span asp-validation-for="Input.ImageFile" class="admin-field-error"></span>
                <p class="admin-hint">JPG, PNG ili WEBP, do 5 MB.</p>
            </div>

            <div class="admin-field">
                <label>Opća fotografija</label>
                @if (!string.IsNullOrEmpty(Model.ExistingPhotoUrl))
                {
                    <img src="@Model.ExistingPhotoUrl" alt="" class="admin-thumb-preview" />
                }
                <input asp-for="Input.PhotoFile" type="file" accept=".jpg,.jpeg,.png,.webp" />
                <span asp-validation-for="Input.PhotoFile" class="admin-field-error"></span>
                <p class="admin-hint">Prikazuje se uz tehničke informacije na javnoj stranici. JPG, PNG ili WEBP, do 5 MB.</p>
            </div>

            <div class="admin-section">
                <p class="admin-section-label">Shema slaganja</p>
                <p class="admin-hint">Dodajte jednu ili više fotografija sheme slaganja.</p>

                @if (Model.Input.ExistingLayoutImages.Any())
                {
                    <div class="admin-image-list">
                        @for (var i = 0; i < Model.Input.ExistingLayoutImages.Count; i++)
                        {
                            <div class="admin-image-row">
                                <img src="@Model.Input.ExistingLayoutImages[i].ImageUrl" alt="" class="admin-thumb-preview" />
                                <input type="hidden" asp-for="Input.ExistingLayoutImages[i].Id" />
                                <input type="hidden" asp-for="Input.ExistingLayoutImages[i].ImageUrl" />
                                <label class="admin-image-sort">
                                    Redoslijed
                                    <input asp-for="Input.ExistingLayoutImages[i].SortOrder" type="number" />
                                </label>
                                <label class="admin-image-delete">
                                    <input asp-for="Input.ExistingLayoutImages[i].Delete" type="checkbox" /> Obriši
                                </label>
                            </div>
                        }
                    </div>
                }

                <div class="admin-field">
                    <label asp-for="Input.NewLayoutImageFiles">Dodaj fotografije</label>
                    <input asp-for="Input.NewLayoutImageFiles" type="file" accept=".jpg,.jpeg,.png,.webp" multiple />
                    <span asp-validation-for="Input.NewLayoutImageFiles" class="admin-field-error"></span>
                    <p class="admin-hint">JPG, PNG ili WEBP, do 5 MB po slici. Moguć višestruki odabir.</p>
                </div>
            </div>

            <div class="admin-section">
                <p class="admin-section-label">Primjeri</p>
                <p class="admin-hint">Svaki primjer je naziv (npr. "Blue") i fotografija te varijante. Dodajte ih klikom na "+ Dodaj primjer".</p>

                @if (Model.Input.ExistingExamples.Any())
                {
                    <div class="admin-example-list" id="existing-example-list">
                        @for (var i = 0; i < Model.Input.ExistingExamples.Count; i++)
                        {
                            <div class="admin-example-row">
                                <img src="@Model.Input.ExistingExamples[i].ImageUrl" alt="" class="admin-thumb-preview" />
                                <input type="hidden" asp-for="Input.ExistingExamples[i].Id" />
                                <input type="hidden" asp-for="Input.ExistingExamples[i].ImageUrl" />
                                <div class="admin-example-field">
                                    <label>Naziv</label>
                                    <input asp-for="Input.ExistingExamples[i].Name" />
                                </div>
                                <label class="admin-image-delete">
                                    <input asp-for="Input.ExistingExamples[i].Delete" type="checkbox" /> Obriši
                                </label>
                            </div>
                        }
                    </div>
                }

                <div class="admin-example-list" id="new-example-list"></div>

                <template id="new-example-template">
                    <div class="admin-example-row admin-new-example-row">
                        <div class="admin-example-field">
                            <label>Naziv</label>
                            <input type="text" class="js-new-example-name" placeholder="npr. Blue" />
                        </div>
                        <div class="admin-example-field">
                            <label>Fotografija</label>
                            <input type="file" accept=".jpg,.jpeg,.png,.webp" class="js-new-example-file" />
                        </div>
                        <button type="button" class="admin-delete-btn js-remove-example-row">Ukloni</button>
                    </div>
                </template>

                <button type="button" id="add-example-btn" class="admin-btn admin-add-btn">+ Dodaj primjer</button>
            </div>
        </div>

        <div class="admin-toggle-panel" data-panel="reljefne">
            <div class="admin-field">
                <label asp-for="Input.AvailableColors">Dostupne boje</label>
                <input asp-for="Input.AvailableColors" placeholder="npr. vidi Karta boja" />
            </div>

            <div class="admin-section">
                <p class="admin-section-label">Fotografije</p>
                <p class="admin-hint">Dodajte jednu ili više fotografija.</p>

                @if (Model.Input.ExistingReljefneImages.Any())
                {
                    <div class="admin-image-list">
                        @for (var i = 0; i < Model.Input.ExistingReljefneImages.Count; i++)
                        {
                            <div class="admin-image-row">
                                <img src="@Model.Input.ExistingReljefneImages[i].ImageUrl" alt="" class="admin-thumb-preview" />
                                <input type="hidden" asp-for="Input.ExistingReljefneImages[i].Id" />
                                <input type="hidden" asp-for="Input.ExistingReljefneImages[i].ImageUrl" />
                                <label class="admin-image-sort">
                                    Redoslijed
                                    <input asp-for="Input.ExistingReljefneImages[i].SortOrder" type="number" />
                                </label>
                                <label class="admin-image-delete">
                                    <input asp-for="Input.ExistingReljefneImages[i].Delete" type="checkbox" /> Obriši
                                </label>
                            </div>
                        }
                    </div>
                }

                <div class="admin-field">
                    <label asp-for="Input.NewReljefneImageFiles">Dodaj fotografije</label>
                    <input asp-for="Input.NewReljefneImageFiles" type="file" accept=".jpg,.jpeg,.png,.webp" multiple />
                    <span asp-validation-for="Input.NewReljefneImageFiles" class="admin-field-error"></span>
                    <p class="admin-hint">JPG, PNG ili WEBP, do 5 MB po slici. Moguć višestruki odabir.</p>
                </div>
            </div>
        </div>

        <div class="admin-field">
            <label asp-for="Input.OtherInfo">Ostali info</label>
            <textarea asp-for="Input.OtherInfo" rows="3"></textarea>
        </div>

        <div class="admin-field">
            <label asp-for="Input.Price">Cijena</label>
            <textarea asp-for="Input.Price" rows="3" placeholder="Više redaka dopušteno"></textarea>
        </div>

        <div class="admin-field">
            <label asp-for="Input.SortOrder">Redoslijed</label>
            <input asp-for="Input.SortOrder" type="number" />
        </div>

        <div class="admin-actions">
            <button type="submit" class="admin-btn admin-btn-primary">Spremi</button>
            <a asp-page="Index" class="admin-btn">Odustani</a>
        </div>
    </form>
</div>

@section Scripts {
    <script src="~/js/admin-toggle.js" asp-append-version="true"></script>
    <script src="~/js/admin.js" asp-append-version="true"></script>
}
```

- [ ] **Step 5: Delete the old `Oblici` folder**

```bash
git rm Plocica/Pages/Admin/Oblici/Index.cshtml Plocica/Pages/Admin/Oblici/Index.cshtml.cs Plocica/Pages/Admin/Oblici/Edit.cshtml Plocica/Pages/Admin/Oblici/Edit.cshtml.cs
```

(This task does **not** yet touch `Pages/Admin/Index.cshtml` or `Pages/Shared/_AdminLayout.cshtml` — they still link to `/Admin/Oblici/Index` until Task 4. That's fine; those are just dead links until then, not compile errors.)

- [ ] **Step 6: Build and confirm remaining errors are only in the public site**

Run: `dotnet build`
Expected: FAIL, with remaining errors now confined to `Pages/Shared/_ShapeGrid.cshtml` and `Pages/Kolekcije.cshtml.cs` (both still reference `Shape.LayoutScheme`/expect the old include shape). Confirm no errors remain about `Oblici` or about `Finish`/`LayoutScheme` inside anything under `Pages/Admin`.

- [ ] **Step 7: Commit**

```bash
git add Plocica/Pages/Admin/Kolekcija Plocica/Pages/Admin/Oblici
git commit -m "feat: rename admin Oblici section to Kolekcija with category-aware edit form"
```

---

## Task 3: Public site — layout gallery, Reljefne gallery, drop the Finish row

**Files:**
- Modify: `Plocica/Pages/Shared/_ShapeGrid.cshtml`
- Modify: `Plocica/Pages/Kolekcije.cshtml`
- Modify: `Plocica/Pages/Kolekcije.cshtml.cs`
- Modify: `Plocica/wwwroot/css/components.css`

**Interfaces:**
- Consumes: `Shape.GalleryImages`, `ShapeGalleryImage.KindLayout`/`KindReljefna` from Task 1.

- [ ] **Step 1: Rewrite `Plocica/Pages/Shared/_ShapeGrid.cshtml`** — image row for "Shema slaganja" instead of text, "Završna obrada" row removed:

```html
@model List<Plocica.Models.Shape>

<div class="shape-icon-grid">
    @foreach (var shape in Model)
    {
        <button type="button" class="shape-icon-tile" aria-expanded="false" aria-controls="shape-details-@shape.Id">
            @if (!string.IsNullOrEmpty(shape.ImageUrl))
            {
                <img src="@shape.ImageUrl" alt="@shape.Name oblik" loading="lazy">
            }
            else
            {
                <div class="shape-icon-empty"></div>
            }
            <span class="shape-icon-name">@shape.Name</span>
        </button>
    }
</div>

<div class="shape-details-wrap">
    @foreach (var shape in Model)
    {
        var layoutImages = shape.GalleryImages
            .Where(g => g.Kind == Plocica.Models.ShapeGalleryImage.KindLayout)
            .OrderBy(g => g.SortOrder)
            .ToList();

        <div class="shape-details" id="shape-details-@shape.Id" hidden>
            <div class="shape-details-grid">
                @if (!string.IsNullOrEmpty(shape.PhotoUrl))
                {
                    <figure class="shape-details-photo">
                        <img src="@shape.PhotoUrl" alt="@shape.Name u prostoru" loading="lazy">
                    </figure>
                }
                <div class="shape-details-info">
                    <p class="eyebrow">Tehničke informacije</p>
                    <h3>@shape.Name</h3>
                    <dl class="spec-dl">
                        @if (!string.IsNullOrEmpty(shape.Thickness))
                        {
                            <div><dt>Debljina</dt><dd>@shape.Thickness</dd></div>
                        }
                        @if (!string.IsNullOrEmpty(shape.Dimensions))
                        {
                            <div><dt>Dimenzija</dt><dd>@shape.Dimensions</dd></div>
                        }
                        @if (!string.IsNullOrEmpty(shape.OtherInfo))
                        {
                            <div><dt>Ostale info</dt><dd>@shape.OtherInfo</dd></div>
                        }
                        @if (!string.IsNullOrEmpty(shape.Price))
                        {
                            <div><dt>Cijena</dt><dd>@shape.Price</dd></div>
                        }
                    </dl>
                </div>
            </div>

            @if (layoutImages.Any())
            {
                <div class="shape-examples">
                    <p class="eyebrow">Shema slaganja</p>
                    <div class="shape-examples-grid">
                        @foreach (var image in layoutImages)
                        {
                            <figure class="shape-example">
                                <img src="@image.ImageUrl" alt="Shema slaganja @shape.Name" loading="lazy">
                            </figure>
                        }
                    </div>
                </div>
            }

            @if (shape.Examples.Any())
            {
                <div class="shape-examples">
                    <p class="eyebrow">Primjeri</p>
                    <div class="shape-examples-grid">
                        @foreach (var example in shape.Examples.OrderBy(e => e.SortOrder))
                        {
                            <figure class="shape-example">
                                <img src="@example.ImageUrl" alt="@example.Name" loading="lazy">
                                <figcaption>@example.Name</figcaption>
                            </figure>
                        }
                    </div>
                </div>
            }

            <button type="button" class="shape-details-close">Zatvori</button>
        </div>
    }
</div>
```

- [ ] **Step 2: Update the Reljefne section in `Plocica/Pages/Kolekcije.cshtml`** — replace the existing `<div class="spec-list">...</div>` block (inside `<section id="reljefne">`) with:

```html
        <div class="spec-list">
            @foreach (var shape in Model.ReljefneShapes)
            {
                var reljefneImages = shape.GalleryImages
                    .Where(g => g.Kind == Plocica.Models.ShapeGalleryImage.KindReljefna)
                    .OrderBy(g => g.SortOrder)
                    .ToList();

                <div class="spec-row">
                    <span class="spec-name">@shape.Name</span>
                    <span class="spec-dim">@shape.OtherInfo</span>
                    <span class="spec-price"><span class="spec-label">Cijena</span>@shape.Price</span>
                    @if (!string.IsNullOrEmpty(shape.AvailableColors))
                    {
                        <span class="spec-colors"><span class="spec-label">Boje iz ponude</span>@shape.AvailableColors</span>
                    }
                    @if (reljefneImages.Any())
                    {
                        <div class="shape-examples-grid spec-gallery">
                            @foreach (var image in reljefneImages)
                            {
                                <figure class="shape-example">
                                    <img src="@image.ImageUrl" alt="@shape.Name" loading="lazy">
                                </figure>
                            }
                        </div>
                    }
                </div>
            }
        </div>
```

- [ ] **Step 3: Add the Include in `Plocica/Pages/Kolekcije.cshtml.cs`** — change the `OnGet` method's first line:

```csharp
    public void OnGet()
    {
        var shapes = _db.Shapes.Include(s => s.Examples).Include(s => s.GalleryImages);
        ObliciShapes = shapes.Where(s => s.Collection == "oblici").OrderBy(s => s.SortOrder).ToList();
        OslikaneShapes = shapes.Where(s => s.Collection == "oslikane").OrderBy(s => s.SortOrder).ToList();
        ReljefneShapes = shapes.Where(s => s.Collection == "reljefne").OrderBy(s => s.SortOrder).ToList();
        Colors = _db.Colors.OrderBy(c => c.SortOrder).ToList();
    }
```

- [ ] **Step 4: Add CSS so the Reljefne gallery spans the full `.spec-row` grid width** — in `Plocica/wwwroot/css/components.css`, immediately after the `.spec-row .spec-colors { ... }` rule (around line 712), add:

```css
.spec-row .spec-gallery {
  grid-column: 1 / -1;
  margin-top: var(--space-4);
}
```

- [ ] **Step 5: Build and confirm success**

Run: `dotnet build`
Expected: PASS (0 errors). This is the first fully green build since Task 1 — all `Finish`/`LayoutScheme` references are gone and every consumer of `Shape` compiles against the new shape.

- [ ] **Step 6: Commit**

```bash
git add Plocica/Pages/Shared/_ShapeGrid.cshtml Plocica/Pages/Kolekcije.cshtml Plocica/Pages/Kolekcije.cshtml.cs Plocica/wwwroot/css/components.css
git commit -m "feat: render shema slaganja and Reljefne photo galleries on the public site"
```

---

## Task 4: EF Core migration

**Files:**
- Create: `Plocica/Migrations/<timestamp>_RestructureShapeCategories.cs`
- Create: `Plocica/Migrations/<timestamp>_RestructureShapeCategories.Designer.cs`
- Modify: `Plocica/Migrations/AppDbContextModelSnapshot.cs`

**Interfaces:**
- Consumes: the final `Shape`/`ShapeGalleryImage` model from Task 1 (requires the Task 3 green build — `dotnet ef` builds the whole project before diffing the model).

- [ ] **Step 1: Confirm the `dotnet-ef` tool is available**

Run: `dotnet ef --version` (from `c:\Users\korisnik\Gita\pločice\Plocica`)
Expected: prints a version like `Entity Framework Core .NET Command-line Tools 8.0.11`. If it instead says the command was not found, install it first:

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

- [ ] **Step 2: Generate the migration**

Run (from `c:\Users\korisnik\Gita\pločice\Plocica`): `dotnet ef migrations add RestructureShapeCategories`
Expected: two new files appear under `Migrations/` (`..._RestructureShapeCategories.cs` and its `.Designer.cs`), and `AppDbContextModelSnapshot.cs` is updated.

- [ ] **Step 3: Inspect the generated migration's `Up` method**

Open the new `..._RestructureShapeCategories.cs` and confirm `Up` contains exactly three kinds of operations — dropping `Finish`, dropping `LayoutScheme`, and creating `ShapeGalleryImages` (with an FK to `Shapes` and an index on `ShapeId`), matching this shape:

```csharp
migrationBuilder.DropColumn(name: "Finish", table: "Shapes");
migrationBuilder.DropColumn(name: "LayoutScheme", table: "Shapes");

migrationBuilder.CreateTable(
    name: "ShapeGalleryImages",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        ShapeId = table.Column<int>(type: "int", nullable: false),
        Kind = table.Column<string>(type: "nvarchar(max)", nullable: false),
        ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
        SortOrder = table.Column<int>(type: "int", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_ShapeGalleryImages", x => x.Id);
        table.ForeignKey(
            name: "FK_ShapeGalleryImages_Shapes_ShapeId",
            column: x => x.ShapeId,
            principalTable: "Shapes",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateIndex(
    name: "IX_ShapeGalleryImages_ShapeId",
    table: "ShapeGalleryImages",
    column: "ShapeId");
```

If EF generated anything beyond these three operations, stop and investigate — it means the model has unrelated drift from a prior uncommitted change; do not proceed until the diff matches exactly what's described above.

- [ ] **Step 4: Build and confirm success**

Run: `dotnet build`
Expected: PASS (0 errors).

- [ ] **Step 5: Apply the migration to the local dev DB and confirm no data loss on existing rows**

Run the app once: `dotnet run` (from `c:\Users\korisnik\Gita\pločice\Plocica`), then stop it (Ctrl+C). `DbInitializer.Seed` calls `db.Database.Migrate()` on startup, so this applies the new migration automatically. Watch the console output for any migration error. Then confirm the existing seeded shapes are intact by browsing to `https://localhost:<port>/kolekcije` — the four Oblici, three Oslikane, and two Reljefne entries (Reljefne iz kalupa, Ručno rezbarene) should still show their names/prices as before (their `Finish` value is simply gone, everything else unaffected).

- [ ] **Step 6: Commit**

```bash
git add Plocica/Migrations
git commit -m "feat: add RestructureShapeCategories migration"
```

---

## Task 5: Dashboard and nav — "Oblici" label becomes "Kolekcija"

**Files:**
- Modify: `Plocica/Pages/Admin/Index.cshtml`
- Modify: `Plocica/Pages/Shared/_AdminLayout.cshtml`

- [ ] **Step 1: Update the dashboard card in `Plocica/Pages/Admin/Index.cshtml`** — change:

```html
        <a class="admin-dash-card" asp-page="/Admin/Oblici/Index">
            <span class="admin-dash-count">@Model.ShapeCount</span>
            <span class="admin-dash-label">Oblici</span>
            <span class="admin-dash-link">Uredi →</span>
        </a>
```

to:

```html
        <a class="admin-dash-card" asp-page="/Admin/Kolekcija/Index">
            <span class="admin-dash-count">@Model.ShapeCount</span>
            <span class="admin-dash-label">Kolekcija</span>
            <span class="admin-dash-link">Uredi →</span>
        </a>
```

- [ ] **Step 2: Update the nav link in `Plocica/Pages/Shared/_AdminLayout.cshtml`** — change:

```html
                    <a asp-page="/Admin/Oblici/Index">Oblici</a>
```

to:

```html
                    <a asp-page="/Admin/Kolekcija/Index">Kolekcija</a>
```

- [ ] **Step 3: Build and confirm success**

Run: `dotnet build`
Expected: PASS (0 errors).

- [ ] **Step 4: Commit**

```bash
git add Plocica/Pages/Admin/Index.cshtml Plocica/Pages/Shared/_AdminLayout.cshtml
git commit -m "feat: rename Oblici to Kolekcija in admin dashboard and nav"
```

---

## Task 6: Plain admin buttons everywhere (ditch `.btn`/`.btn-cta` in the admin panel)

**Files:**
- Modify: `Plocica/wwwroot/css/admin.css`
- Modify: `Plocica/Pages/Admin/Login.cshtml`
- Modify: `Plocica/Pages/Admin/Boje/Index.cshtml`
- Modify: `Plocica/Pages/Admin/Boje/Edit.cshtml`
- Modify: `Plocica/Pages/Admin/Projekti/Index.cshtml`
- Modify: `Plocica/Pages/Admin/Projekti/Edit.cshtml`
- Rename: `Plocica/wwwroot/js/admin-color-toggle.js` → `Plocica/wwwroot/js/admin-toggle.js`

**Interfaces:**
- Produces: `.admin-btn` / `.admin-btn-primary` CSS classes, already referenced by Task 2's `Kolekcija/Index.cshtml` and `Kolekcija/Edit.cshtml`.

- [ ] **Step 1: Add plain button styles to `Plocica/wwwroot/css/admin.css`** — append at the end of the file:

```css

/* ---------- Admin gumbi (jednostavni — stilizirana verzija ostaje samo na javnoj stranici) ---------- */

.admin-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5em;
  padding: 0.65em 1.25em;
  border: 1px solid var(--line);
  border-radius: 0;
  background: var(--paper);
  color: var(--ink);
  font: inherit;
  font-size: 0.9375rem;
  cursor: pointer;
  text-decoration: none;
  transition: border-color var(--dur-fast) var(--ease-spring), background var(--dur-fast) var(--ease-spring);
}

.admin-btn:hover,
.admin-btn:focus-visible {
  border-color: var(--ink);
  background: var(--paper-2);
}

.admin-btn-primary {
  background: var(--ink);
  color: var(--paper);
  border-color: var(--ink);
}

.admin-btn-primary:hover,
.admin-btn-primary:focus-visible {
  background: color-mix(in srgb, var(--ink) 88%, white);
  border-color: color-mix(in srgb, var(--ink) 88%, white);
}
```

- [ ] **Step 2: Replace button classes in `Plocica/Pages/Admin/Login.cshtml`** — change:

```html
        <button type="submit" class="btn btn-cta">Prijavi se</button>
```

to:

```html
        <button type="submit" class="admin-btn admin-btn-primary">Prijavi se</button>
```

- [ ] **Step 3: Replace button classes in `Plocica/Pages/Admin/Boje/Index.cshtml`** — change:

```html
        <a asp-page="Edit" class="btn btn-cta">Nova</a>
```

to:

```html
        <a asp-page="Edit" class="admin-btn admin-btn-primary">Nova</a>
```

- [ ] **Step 4: Replace button classes and the toggle script reference in `Plocica/Pages/Admin/Boje/Edit.cshtml`** — change:

```html
        <div class="admin-actions">
            <button type="submit" class="btn btn-cta">Spremi</button>
            <a asp-page="Index" class="btn">Odustani</a>
        </div>
    </form>
</div>

@section Scripts {
    <script src="~/js/admin-color-toggle.js" asp-append-version="true"></script>
}
```

to:

```html
        <div class="admin-actions">
            <button type="submit" class="admin-btn admin-btn-primary">Spremi</button>
            <a asp-page="Index" class="admin-btn">Odustani</a>
        </div>
    </form>
</div>

@section Scripts {
    <script src="~/js/admin-toggle.js" asp-append-version="true"></script>
}
```

- [ ] **Step 5: Replace button classes in `Plocica/Pages/Admin/Projekti/Index.cshtml`** — change:

```html
        <a asp-page="Edit" class="btn btn-cta">Novi</a>
```

to:

```html
        <a asp-page="Edit" class="admin-btn admin-btn-primary">Novi</a>
```

- [ ] **Step 6: Replace button classes in `Plocica/Pages/Admin/Projekti/Edit.cshtml`** — change:

```html
        <div class="admin-actions">
            <button type="submit" class="btn btn-cta">Spremi</button>
            <a asp-page="Index" class="btn">Odustani</a>
        </div>
```

to:

```html
        <div class="admin-actions">
            <button type="submit" class="admin-btn admin-btn-primary">Spremi</button>
            <a asp-page="Index" class="admin-btn">Odustani</a>
        </div>
```

- [ ] **Step 7: Rename the toggle script**

```bash
git mv Plocica/wwwroot/js/admin-color-toggle.js Plocica/wwwroot/js/admin-toggle.js
```

(No content change needed — the script already works generically off `data-toggle-target` / `.admin-toggle-panel`, which is exactly what Task 2's Kolekcija Edit page also uses.)

- [ ] **Step 8: Build and confirm success**

Run: `dotnet build`
Expected: PASS (0 errors).

- [ ] **Step 9: Manual visual check**

Run: `dotnet run`, log into `/Admin/Login`, and open Login, Boje (Index + Edit), Projekti (Index + Edit), and Kolekcija (Index + Edit). Confirm every button/link in the admin panel is the plain bordered style (no clip-path corner, no gradient sheen) and that spacing between buttons and fields looks the same across all these pages. Then open the public site (`/`, `/kolekcije`) and confirm the `.btn-cta` clip-path button still appears there unchanged (e.g. any public call-to-action button) — it must be untouched outside the admin panel.

- [ ] **Step 10: Commit**

```bash
git add Plocica/wwwroot/css/admin.css Plocica/Pages/Admin/Login.cshtml Plocica/Pages/Admin/Boje Plocica/Pages/Admin/Projekti Plocica/wwwroot/js/admin-toggle.js
git commit -m "style: replace stylised CTA button with plain admin buttons across the admin panel"
```

---

## Task 7: End-to-end manual verification

**Files:** none (verification only).

- [ ] **Step 1: Create an Oblici item** — log into `/Admin/Kolekcija/Index`, click "Novi", pick "Oblici", fill Naziv/Debljina/Dimenzije, upload a Skica and an Opća fotografija, add 2 "Shema slaganja" photos, add one Primjer (name + photo), save. Confirm it appears under the "Oblici" table on the Index page and that editing it again shows all uploaded images and the Primjer.

- [ ] **Step 2: Create a Ručno oslikane item** — same as Step 1 but category "Ručno oslikane". Confirm it lists under the "Ručno oslikane" table.

- [ ] **Step 3: Edit an existing Reljefne item** — open "Reljefne iz kalupa" or "Ručno rezbarene", add 2-3 photos under "Fotografije", confirm "Dostupne boje" field is present and the Oblici-only fields (Skica, Opća fotografija, Shema slaganja, Primjeri) are not shown. Save.

- [ ] **Step 4: Verify the public `/kolekcije` page** — confirm the new Oblici/Oslikane items appear in their grids, clicking a tile shows the "Shema slaganja" images and the Primjer; confirm the two Reljefne rows now show their added photos underneath the existing text row, and that "Završna obrada" no longer appears anywhere.

- [ ] **Step 5: Delete a gallery image** — edit any item with gallery photos, check "Obriši" on one existing image, save, confirm it's gone from both the admin form and the public page.

- [ ] **Step 6: No follow-up commit** — this task only verifies; if any step fails, fix the underlying task and re-run this verification, then commit the fix under that task's message.

---

## Self-Review Notes

- **Spec coverage:** data model (Task 1), admin structure/fields/toggle (Task 2), admin visual consistency (Task 6), public site galleries (Task 3), migration (Task 4), out-of-scope items (lightbox, drag-reorder, category renaming UI) — none added, consistent with the spec's "Out of scope" section.
- **Type consistency:** `ShapeGalleryImage.KindLayout`/`KindReljefna` constants are defined once in Task 1 and referenced by identical fully-qualified name in Task 2 (C#) and Task 3 (Razor, `Plocica.Models.ShapeGalleryImage.KindLayout`/`KindReljefna`) — no ad-hoc string literals duplicated elsewhere.
- **No placeholders:** every step has runnable code or an exact command; no "TBD"/"similar to above" shortcuts.
