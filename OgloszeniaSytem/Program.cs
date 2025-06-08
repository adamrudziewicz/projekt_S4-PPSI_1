using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OgloszeniaSytem.Data;
using OgloszeniaSytem.Models;
using OgloszeniaSytem.Services;
using Serilog;
using System.Globalization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja Serilog (Logger)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Entity Framework z SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ogloszenia.db"));

// ASP.NET Core Identity (uwierzytelnianie)
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// MVC Framework
builder.Services.AddControllersWithViews();

// Memory Cache
builder.Services.AddMemoryCache();

// Lokalizacja (nierozwinieta)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "pl-PL", "en-US" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

var polishCulture = new CultureInfo("pl-PL");
CultureInfo.DefaultThreadCurrentCulture = polishCulture;
CultureInfo.DefaultThreadCurrentUICulture = polishCulture;

// Dependency Injection (Services)
builder.Services.AddScoped<IListingService, ListingService>();
//builder.Services.AddScoped<IEmailService, EmailService>(); // niewykorzystane
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<ISimilarListingService, SimilarListingService>();
builder.Services.AddHttpClient();


var app = builder.Build();

// Konfiguracja pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseRouting();

// Lokalizacja middleware
app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

// Routing z pretty URLs
app.MapControllerRoute(
    name: "listing",
    pattern: "listing/{id:int}/{slug?}",
    defaults: new { controller = "Listing", action = "Details" });

app.MapControllerRoute(
    name: "category",
    pattern: "category/{kategoria}/{lokalizacja?}",
    defaults: new { controller = "Listing", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Inicjalizacja bazy danych z obsługą błędów
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Inicjalizacja bazy danych...");
        await context.Database.EnsureCreatedAsync();
        
        logger.LogInformation("Wypełnianie danymi startowymi...");
        await SeedData.Initialize(scope.ServiceProvider);
        
        logger.LogInformation("Baza danych zainicjalizowana pomyślnie");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Błąd podczas inicjalizacji bazy danych");
    throw;
}

app.Run();