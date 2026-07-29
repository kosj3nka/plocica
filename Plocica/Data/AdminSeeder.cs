using Microsoft.AspNetCore.Identity;
using Plocica.Models;

namespace Plocica.Data;

public static class AdminSeeder
{
    public static void Seed(AppDbContext db, IConfiguration config)
    {
        if (db.AdminUsers.Any())
        {
            return;
        }

        var username = config["Admin:InitialUsername"];
        var password = config["Admin:InitialPassword"];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            // Nema inicijalnih vjerodajnica u konfiguraciji (User Secrets / env varijable) — admin račun se ne kreira.
            return;
        }

        var hasher = new PasswordHasher<AdminUser>();
        var admin = new AdminUser { Username = username };
        admin.PasswordHash = hasher.HashPassword(admin, password);

        db.AdminUsers.Add(admin);
        db.SaveChanges();
    }
}
