using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgloszeniaSytem.Data;
using OgloszeniaSytem.Models;
using System.Diagnostics;

namespace OgloszeniaSytem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Ładowanie strony głównej...");

                // Pobierz kategorie
                var kategorie = await _context.Kategorie.ToListAsync();
                _logger.LogInformation("Znaleziono {Count} kategorii", kategorie.Count);

                // Pobierz lokalizacje - DODANE
                var lokalizacje = await _context.Lokalizacje.ToListAsync();
                _logger.LogInformation("Znaleziono {Count} lokalizacji", lokalizacje.Count);

                // Pobierz najnowsze ogłoszenia
                var najnowszeOgloszenia = await _context.Ogloszenia
                    .Include(o => o.Kategoria)
                    .Include(o => o.Lokalizacja)
                    .Include(o => o.Autor)
                    .Include(o => o.Zdjecia) // DODANE - żeby pobierać zdjęcia
                    .Where(o => o.CzyAktywne)
                    .OrderByDescending(o => o.DataPublikacji)
                    .Take(6)
                    .ToListAsync();

                _logger.LogInformation("Znaleziono {Count} ogłoszeń", najnowszeOgloszenia.Count);

                // Ustaw ViewBag - zawsze ustaw jako listy, nawet jeśli puste
                ViewBag.Kategorie = kategorie ?? new List<Category>();
                ViewBag.Lokalizacje = lokalizacje ?? new List<Location>(); // DODANE
                ViewBag.NajnowszeOgloszenia = najnowszeOgloszenia ?? new List<Listing>();

                // Debugowanie - sprawdź czy ogłoszenia mają poprawne dane
                foreach (var ogloszenie in najnowszeOgloszenia)
                {
                    _logger.LogDebug("Ogłoszenie ID: {Id}, Tytuł: {Tytul}, Cena: {Cena}", 
                        ogloszenie.Id, ogloszenie.Tytul, ogloszenie.Cena);
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ładowania strony głównej");
                
                // W przypadku błędu ustaw puste kolekcje
                ViewBag.Kategorie = new List<Category>();
                ViewBag.Lokalizacje = new List<Location>(); // DODANE
                ViewBag.NajnowszeOgloszenia = new List<Listing>();
                ViewBag.ErrorMessage = "Wystąpił błąd podczas ładowania danych. Spróbuj odświeżyć stronę.";
                
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorModel = new Error 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = "Wystąpił nieoczekiwany błąd",
                StatusCode = HttpContext.Response.StatusCode
            };
            
            return View(errorModel);
        }
    }
}