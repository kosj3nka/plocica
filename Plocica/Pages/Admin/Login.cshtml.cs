using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plocica.Data;
using Plocica.Models;
using Plocica.Services;

namespace Plocica.Pages.Admin;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly LoginThrottleService _throttle;

    public LoginModel(AppDbContext db, LoginThrottleService throttle)
    {
        _db = db;
        _throttle = throttle;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Unesite korisničko ime.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unesite lozinku.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (_throttle.IsLockedOut(out var remaining))
        {
            ErrorMessage = $"Previše neuspjelih pokušaja. Pokušajte ponovno za {Math.Ceiling(remaining.TotalSeconds)} s.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var admin = _db.AdminUsers.FirstOrDefault(u => u.Username == Input.Username);
        var hasher = new PasswordHasher<AdminUser>();

        var verifyResult = admin is not null
            ? hasher.VerifyHashedPassword(admin, admin.PasswordHash, Input.Password)
            : PasswordVerificationResult.Failed;

        if (admin is null || verifyResult == PasswordVerificationResult.Failed)
        {
            _throttle.RegisterFailure();
            ErrorMessage = "Pogrešno korisničko ime ili lozinka.";
            return Page();
        }

        _throttle.RegisterSuccess();

        var claims = new List<Claim> { new(ClaimTypes.Name, admin.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return LocalRedirect(returnUrl ?? "/Admin");
    }
}
