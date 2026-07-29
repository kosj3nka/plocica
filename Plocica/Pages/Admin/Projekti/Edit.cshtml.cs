using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin.Projekti;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IBlobStorageService _blob;

    public EditModel(AppDbContext db, IBlobStorageService blob)
    {
        _db = db;
        _blob = blob;
    }

    public int? Id { get; set; }

    [BindProperty]
    public ProjectInput Input { get; set; } = new();

    public class ProjectInput
    {
        [Required(ErrorMessage = "Unesite naziv.")]
        public string Title { get; set; } = string.Empty;

        public string? Location { get; set; }

        [Required(ErrorMessage = "Unesite tekst.")]
        public string Text { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public List<ExistingImageInput> ExistingImages { get; set; } = new();

        public List<IFormFile>? NewImageFiles { get; set; }
    }

    public class ExistingImageInput
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool Delete { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Id = id;

        if (id is not null)
        {
            var project = await _db.Projects.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id.Value);
            if (project is null)
            {
                return RedirectToPage("Index");
            }

            Input = new ProjectInput
            {
                Title = project.Title,
                Location = project.Location,
                Text = project.Text,
                SortOrder = project.SortOrder,
                ExistingImages = project.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ExistingImageInput { Id = i.Id, Url = i.Url, SortOrder = i.SortOrder })
                    .ToList(),
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        Id = id;
        Project? project = null;

        if (id is not null)
        {
            project = await _db.Projects.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id.Value);
            if (project is null)
            {
                return RedirectToPage("Index");
            }
        }

        if (!ModelState.IsValid)
        {
            if (project is not null)
            {
                Input.ExistingImages = project.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ExistingImageInput { Id = i.Id, Url = i.Url, SortOrder = i.SortOrder })
                    .ToList();
            }

            return Page();
        }

        if (project is null)
        {
            project = new Project();
            _db.Projects.Add(project);
        }

        project.Title = Input.Title;
        project.Location = Input.Location;
        project.Text = Input.Text;
        project.SortOrder = Input.SortOrder;

        // Ažuriranje / brisanje postojećih slika.
        foreach (var existing in Input.ExistingImages)
        {
            var image = project.Images.FirstOrDefault(i => i.Id == existing.Id);
            if (image is null)
            {
                continue;
            }

            if (existing.Delete)
            {
                await _blob.DeleteAsync(image.Url);
                _db.ProjectImages.Remove(image);
            }
            else
            {
                image.SortOrder = existing.SortOrder;
            }
        }

        // Upload novih slika.
        if (Input.NewImageFiles is not null)
        {
            var nextSortOrder = project.Images.Any() ? project.Images.Max(i => i.SortOrder) + 1 : 0;

            foreach (var file in Input.NewImageFiles.Where(f => f.Length > 0))
            {
                string url;
                try
                {
                    url = await _blob.UploadAsync(file);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("Input.NewImageFiles", ex.Message);
                    Input.ExistingImages = project.Images
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new ExistingImageInput { Id = i.Id, Url = i.Url, SortOrder = i.SortOrder })
                        .ToList();
                    return Page();
                }

                project.Images.Add(new ProjectImage { Url = url, SortOrder = nextSortOrder });
                nextSortOrder++;
            }
        }

        await _db.SaveChangesAsync();

        TempData["Message"] = $"Projekt \"{project.Title}\" je spremljen.";
        return RedirectToPage("Index");
    }
}
