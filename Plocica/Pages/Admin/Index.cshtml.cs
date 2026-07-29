using Microsoft.AspNetCore.Mvc.RazorPages;
using Plocica.Data;

namespace Plocica.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public int ShapeCount { get; set; }
    public int ColorCount { get; set; }
    public int ProjectCount { get; set; }

    public void OnGet()
    {
        ShapeCount = _db.Shapes.Count();
        ColorCount = _db.Colors.Count();
        ProjectCount = _db.Projects.Count();
    }
}
