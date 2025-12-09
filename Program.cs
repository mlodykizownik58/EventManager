using EventManagement.Data; // Upewnij siê, ¿e ApplicationDbContext jest w tym namespace
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Dodaj developer-friendly wyj¹tki podczas migracji (tylko w trybie developerskim)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Konfiguracja Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false) // Mo¿esz ustawiæ na `false`, jeœli nie chcesz wymagaæ potwierdzania konta.
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Dodaj kontrolery i widoki (MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Dodanie ról przy starcie aplikacji
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleInitializer.SeedRolesAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // U¿yj endpointu migracji w trybie developerskim
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // W³¹cz HSTS w œrodowisku produkcyjnym
}

app.UseHttpsRedirection(); // Przekierowanie HTTP do HTTPS
app.UseStaticFiles();      // Obs³uga plików statycznych, np. CSS, JS

app.UseRouting();

app.UseAuthentication();   // Uwierzytelnianie u¿ytkownika
app.UseAuthorization();    // Autoryzacja u¿ytkownika

// Mapowanie tras dla MVC i Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Obs³uga stron Razor Pages dla Identity

app.Run();
