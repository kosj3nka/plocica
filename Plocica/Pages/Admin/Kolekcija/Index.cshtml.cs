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
