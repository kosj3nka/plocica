using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Boje;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IBlobStorageService _blob;

    public IndexModel(AppDbContext db, IBlobStorageService blob)
    {
        _db = db;
        _blob = blob;
    }

    public List<ColorItem> Colors { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public void OnGet()
    {
        Colors = _db.Colors.OrderBy(c => c.SortOrder).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var color = await _db.Colors.FindAsync(id);
        if (color is not null)
        {
            await _blob.DeleteAsync(color.ImageUrl);
            _db.Colors.Remove(color);
            await _db.SaveChangesAsync();
            Message = $"Boja \"{color.Code}\" je obrisana.";
        }

        return RedirectToPage("Index");
    }
}
