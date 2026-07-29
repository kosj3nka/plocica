using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Projekti;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IBlobStorageService _blob;

    public IndexModel(AppDbContext db, IBlobStorageService blob)
    {
        _db = db;
        _blob = blob;
    }

    public List<Project> Projects { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    public void OnGet()
    {
        Projects = _db.Projects
            .Include(p => p.Images)
            .OrderBy(p => p.SortOrder)
            .ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var project = await _db.Projects.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (project is not null)
        {
            foreach (var image in project.Images)
            {
                await _blob.DeleteAsync(image.Url);
            }

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            Message = $"Projekt \"{project.Title}\" je obrisan.";
        }

        return RedirectToPage("Index");
    }
}
