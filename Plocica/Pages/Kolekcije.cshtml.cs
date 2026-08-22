using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plocica.Data;
using Plocica.Models;

namespace Plocica.Pages;

public class KolekcijeModel : PageModel
{
    private readonly AppDbContext _db;

    public KolekcijeModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Shape> ObliciShapes { get; set; } = new();
    public List<Shape> OslikaneShapes { get; set; } = new();
    public List<Shape> ReljefneShapes { get; set; } = new();
    public List<ColorItem> Colors { get; set; } = new();

    public void OnGet()
    {
        var shapes = _db.Shapes.Include(s => s.Examples).Include(s => s.GalleryImages);
        ObliciShapes = shapes.Where(s => s.Collection == "oblici").OrderBy(s => s.SortOrder).ToList();
        OslikaneShapes = shapes.Where(s => s.Collection == "oslikane").OrderBy(s => s.SortOrder).ToList();
        ReljefneShapes = shapes.Where(s => s.Collection == "reljefne").OrderBy(s => s.SortOrder).ToList();
        Colors = _db.Colors.OrderBy(c => c.SortOrder).ToList();
    }
}
