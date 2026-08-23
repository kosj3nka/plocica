using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Plocica.Data;
using Plocica.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
                               {
                                   options.Conventions.AuthorizeFolder("/Admin");
                                   options.Conventions.AllowAnonymousToPage("/Admin/Login");
                               });

builder.Services.AddDbContext<AppDbContext>(o =>
                                            o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton(_ =>
                              {
                                  var config = builder.Configuration;
                                  var connectionString = config["Blob:ConnectionString"];

                                  if (!string.IsNullOrWhiteSpace(connectionString))
                                  {
                                      // Lokalni razvoj (npr. Azurite) koristi connection string.
                                      return new BlobServiceClient(connectionString);
                                  }

                                  var accountUrl = config["Blob:AccountUrl"]
                                      ?? throw new InvalidOperationException("Postavi Blob:ConnectionString (lokalno) ili Blob:AccountUrl (produkcija, Managed Identity).");

                                  var managedIdentityClientId = config["Blob:ManagedIdentityClientId"];
                                  var credential = string.IsNullOrWhiteSpace(managedIdentityClientId)
                                      ? new DefaultAzureCredential()
                                      : new ManagedIdentityCredential(managedIdentityClientId);

                                  // Produkcija: bez tajni, autentikacija preko Managed Identity Web App-a.
                                  return new BlobServiceClient(new Uri(accountUrl), credential);
                              });
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<LoginThrottleService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
               {
                   options.LoginPath = "/Admin/Login";
                   options.LogoutPath = "/Admin/Logout";
                   options.AccessDeniedPath = "/Admin/Login";
                   options.ExpireTimeSpan = TimeSpan.FromDays(7);
                   options.SlidingExpiration = true;
                   options.Cookie.HttpOnly = true;
               });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Seed(db);
    AdminSeeder.Seed(db, app.Configuration);

    var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
    blobServiceClient.GetBlobContainerClient(BlobStorageService.ContainerName)
        .CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
}

app.Run();
