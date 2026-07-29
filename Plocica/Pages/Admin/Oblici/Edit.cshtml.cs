using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Oblici;

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

    [BindProperty]
    public ShapeInput Input { get; set; } = new();

    public class ShapeInput
    {
        [Required(ErrorMessage = "Unesite naziv.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Odaberite kolekciju.")]
        public string Collection { get; set; } = "oblici";

        public string? Thickness { get; set; }
        public string? Dimensions { get; set; }
        public string? LayoutScheme { get; set; }
        public string? Finish { get; set; }
        public string? AvailableColors { get; set; }
        public string? OtherInfo { get; set; }
        public string? Price { get; set; }
        public int SortOrder { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Id = id;

        if (id is not null)
        {
            var shape = await _db.Shapes.FindAsync(id.Value);
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
                LayoutScheme = shape.LayoutScheme,
                Finish = shape.Finish,
                AvailableColors = shape.AvailableColors,
                OtherInfo = shape.OtherInfo,
                Price = shape.Price,
                SortOrder = shape.SortOrder,
            };
            ExistingImageUrl = shape.ImageUrl;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        Id = id;
        Shape? shape = null;

        if (id is not null)
        {
            shape = await _db.Shapes.FindAsync(id.Value);
            if (shape is null)
            {
                return RedirectToPage("Index");
            }

            ExistingImageUrl = shape.ImageUrl;
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

        if (shape is null)
        {
            shape = new Shape();
            _db.Shapes.Add(shape);
        }

        shape.Name = Input.Name;
        shape.Collection = Input.Collection;
        shape.Thickness = Input.Thickness;
        shape.Dimensions = Input.Dimensions;
        shape.LayoutScheme = Input.LayoutScheme;
        shape.Finish = Input.Finish;
        shape.AvailableColors = Input.AvailableColors;
        shape.OtherInfo = Input.OtherInfo;
        shape.Price = Input.Price;
        shape.SortOrder = Input.SortOrder;
        shape.ImageUrl = newImageUrl;

        await _db.SaveChangesAsync();

        TempData["Message"] = $"Oblik \"{shape.Name}\" je spremljen.";
        return RedirectToPage("Index");
    }
}
