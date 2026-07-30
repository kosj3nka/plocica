using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plocica.Data;
using Plocica.Models;

namespace Plocica.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(AppDbContext db, ILogger<IndexModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<Shape> ObliciShapes { get; set; } = new();
    public List<Shape> OslikaneShapes { get; set; } = new();
    public List<Shape> ReljefneShapes { get; set; } = new();

    public void OnGet()
    {
        ObliciShapes = _db.Shapes.Where(s => s.Collection == "oblici").OrderBy(s => s.SortOrder).ToList();
        OslikaneShapes = _db.Shapes.Where(s => s.Collection == "oslikane").OrderBy(s => s.SortOrder).ToList();
        ReljefneShapes = _db.Shapes.Where(s => s.Collection == "reljefne").OrderBy(s => s.SortOrder).ToList();
    }
}
