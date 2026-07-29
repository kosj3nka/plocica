using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Oblici;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IBlobStorageService _blob;

    public IndexModel(AppDbContext db, IBlobStorageService blob)
    {
        _db = db;
        _blob = blob;
    }

    public List<Shape> Shapes { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public void OnGet()
    {
        Shapes = _db.Shapes.OrderBy(s => s.Collection).ThenBy(s => s.SortOrder).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var shape = await _db.Shapes.FindAsync(id);
        if (shape is not null)
        {
            await _blob.DeleteAsync(shape.ImageUrl);
            _db.Shapes.Remove(shape);
            await _db.SaveChangesAsync();
            Message = $"Oblik \"{shape.Name}\" je obrisan.";
        }

        return RedirectToPage("Index");
    }
}
