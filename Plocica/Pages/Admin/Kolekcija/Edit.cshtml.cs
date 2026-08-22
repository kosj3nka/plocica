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

        public decimal? Thickness { get; set; }
        public decimal? DimensionHeight { get; set; }
        public decimal? DimensionWidth { get; set; }
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
                DimensionHeight = shape.DimensionHeight,
                DimensionWidth = shape.DimensionWidth,
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
        shape.DimensionHeight = Input.DimensionHeight;
        shape.DimensionWidth = Input.DimensionWidth;
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
