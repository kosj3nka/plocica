using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Boje;

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
    public ColorInput Input { get; set; } = new();

    public class ColorInput
    {
        [Required(ErrorMessage = "Unesite kod.")]
        public string Code { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string Mode { get; set; } = "color";

        public string? Hex { get; set; } = "#7C8A5B";

        public IFormFile? ImageFile { get; set; }

        public int SortOrder { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Id = id;

        if (id is not null)
        {
            var color = await _db.Colors.FindAsync(id.Value);
            if (color is null)
            {
                return RedirectToPage("Index");
            }

            Input = new ColorInput
            {
                Code = color.Code,
                Name = color.Name,
                Mode = !string.IsNullOrEmpty(color.ImageUrl) ? "image" : "color",
                Hex = string.IsNullOrEmpty(color.Hex) ? "#7C8A5B" : color.Hex,
                SortOrder = color.SortOrder,
            };
            ExistingImageUrl = color.ImageUrl;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        Id = id;
        ColorItem? color = null;

        if (id is not null)
        {
            color = await _db.Colors.FindAsync(id.Value);
            if (color is null)
            {
                return RedirectToPage("Index");
            }

            ExistingImageUrl = color.ImageUrl;
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var newImageUrl = ExistingImageUrl;
        var newHex = Input.Hex;

        if (Input.Mode == "image")
        {
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

            if (string.IsNullOrEmpty(newImageUrl))
            {
                ModelState.AddModelError("Input.ImageFile", "Uploadajte sliku ili odaberite boju.");
                return Page();
            }

            newHex = null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(newHex))
            {
                ModelState.AddModelError("Input.Hex", "Odaberite boju.");
                return Page();
            }

            if (!string.IsNullOrEmpty(ExistingImageUrl))
            {
                await _blob.DeleteAsync(ExistingImageUrl);
            }

            newImageUrl = null;
        }

        if (color is null)
        {
            color = new ColorItem();
            _db.Colors.Add(color);
        }

        color.Code = Input.Code;
        color.Name = Input.Name;
        color.Hex = newHex;
        color.ImageUrl = newImageUrl;
        color.SortOrder = Input.SortOrder;

        await _db.SaveChangesAsync();

        TempData["Message"] = $"Boja \"{color.Code}\" je spremljena.";
        return RedirectToPage("Index");
    }
}
