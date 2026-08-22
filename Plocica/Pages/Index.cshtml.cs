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

    // Caps card-link text at maxLength characters without cutting a word in
    // half — backs off to the previous word boundary before appending "…".
    public static string TruncateAtWord(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;

        var truncated = text.Substring(0, maxLength);
        var lastSpace = truncated.LastIndexOf(' ');
        if (lastSpace > 0) truncated = truncated.Substring(0, lastSpace);

        return truncated.TrimEnd(' ', '·') + "…";
    }
}
