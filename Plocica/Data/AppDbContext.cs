using Microsoft.EntityFrameworkCore;
using Plocica.Models;

namespace Plocica.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shape> Shapes => Set<Shape>();
    public DbSet<ColorItem> Colors => Set<ColorItem>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
}
