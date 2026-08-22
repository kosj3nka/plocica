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
