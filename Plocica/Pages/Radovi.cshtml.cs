using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plocica.Data;
using Plocica.Models;

namespace Plocica.Pages;

public class RadoviModel : PageModel
{
    private readonly AppDbContext _db;

    public RadoviModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Project> Projects { get; set; } = new();

    public void OnGet()
    {
        Projects = _db.Projects
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .OrderBy(p => p.SortOrder)
            .ToList();
    }
}
