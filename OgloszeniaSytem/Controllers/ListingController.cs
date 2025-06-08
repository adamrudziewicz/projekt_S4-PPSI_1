using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgloszeniaSytem.Data;
using OgloszeniaSytem.Models;
using OgloszeniaSytem.Services;
using OgloszeniaSytem.ViewModels;

namespace OgloszeniaSytem.Controllers
{
    public class ListingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IListingService _listingService;
        private readonly ILogger<ListingController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ListingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IListingService listingService,
            ILogger<ListingController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _listingService = listingService;
            _logger = logger;
            _environment = environment;
        }

        // GET: Listing
        public async Task<IActionResult> Index(string? kategoria, string? lokalizacja, int page = 1)
        {
            _logger.LogInformation("Pobieranie listy ogłoszeń dla kategorii: {Kategoria}, lokalizacji: {Lokalizacja}", kategoria, lokalizacja);
            
            var ogloszenia = await _listingService.GetOgloszeniaAsync(kategoria, lokalizacja, page);
            
            ViewBag.Kategorie = await _context.Kategorie.ToListAsync();
            ViewBag.Lokalizacje = await _context.Lokalizacje.ToListAsync();
            
            return View(ogloszenia);
        }

        // GET: Listing/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ogloszenie = await _context.Ogloszenia
                .Include(o => o.Autor)
                .Include(o => o.Kategoria)
                .Include(o => o.Lokalizacja)
                .Include(o => o.Zdjecia)
                .Include(o => o.Odpowiedzi)
                    .ThenInclude(odp => odp.Autor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ogloszenie == null) return NotFound();

            // Sprawdź czy użytkownik jest właścicielem
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.IsOwner = currentUser?.Id == ogloszenie.AutorId;

            // Zwiększ licznik wyświetleń
            await _listingService.IncreaseViewCountAsync(id.Value);

            return View(ogloszenie);
        }

        // GET: Listing/MyListings
        [Authorize]
        public async Task<IActionResult> MyListings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var mojeOgloszenia = await _context.Ogloszenia
                .Include(o => o.Kategoria)
                .Include(o => o.Lokalizacja)
                .Include(o => o.Zdjecia)
                .Where(o => o.AutorId == user.Id)
                .OrderByDescending(o => o.DataPublikacji)
                .ToListAsync();

            return View(mojeOgloszenia);
        }

        // GET: Listing/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewBag.Kategorie = await _context.Kategorie.ToListAsync();
            ViewBag.Lokalizacje = await _context.Lokalizacje.ToListAsync();
            return View();
        }

        // POST: Listing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateListingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var ogloszenie = new Listing
                {
                    Tytul = model.Tytul,
                    Opis = model.Opis,
                    Cena = model.Cena,
                    KategoriaId = model.KategoriaId,
                    LokalizacjaId = model.LokalizacjaId,
                    AutorId = user!.Id,
                    DataWaznosci = model.DataWaznosci
                };

                await _listingService.CreateOgloszenieAsync(ogloszenie, model.Zdjecia);
                
                _logger.LogInformation("Utworzono nowe ogłoszenie: {Tytul} przez użytkownika: {UserId}", model.Tytul, user.Id);
                
                TempData["SuccessMessage"] = "Ogłoszenie zostało pomyślnie dodane.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Kategorie = await _context.Kategorie.ToListAsync();
            ViewBag.Lokalizacje = await _context.Lokalizacje.ToListAsync();
            return View(model);
        }

        // GET: Listing/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogloszenie = await _context.Ogloszenia
                .Include(o => o.Zdjecia)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (ogloszenie == null)
            {
                return NotFound();
            }

            // Sprawdź czy użytkownik jest właścicielem ogłoszenia
            var user = await _userManager.GetUserAsync(User);
            if (ogloszenie.AutorId != user?.Id)
            {
                return Forbid();
            }

            var model = new EditListingViewModel
            {
                Id = ogloszenie.Id,
                Tytul = ogloszenie.Tytul,
                Opis = ogloszenie.Opis,
                Cena = ogloszenie.Cena,
                KategoriaId = ogloszenie.KategoriaId,
                LokalizacjaId = ogloszenie.LokalizacjaId,
                DataWaznosci = ogloszenie.DataWaznosci,
                CzyAktywne = ogloszenie.CzyAktywne,
                IstniejaceZdjecia = ogloszenie.Zdjecia.ToList()
            };

            ViewBag.Kategorie = await _context.Kategorie.ToListAsync();
            ViewBag.Lokalizacje = await _context.Lokalizacje.ToListAsync();
            
            return View(model);
        }

        // POST: Listing/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, EditListingViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var ogloszenie = await _context.Ogloszenia
                        .Include(o => o.Zdjecia)
                        .FirstOrDefaultAsync(o => o.Id == id);
                        
                    if (ogloszenie == null)
                    {
                        return NotFound();
                    }

                    // Sprawdź czy użytkownik jest właścicielem ogłoszenia
                    var user = await _userManager.GetUserAsync(User);
                    if (ogloszenie.AutorId != user?.Id)
                    {
                        return Forbid();
                    }

                    // Aktualizuj dane ogłoszenia
                    ogloszenie.Tytul = model.Tytul;
                    ogloszenie.Opis = model.Opis;
                    ogloszenie.Cena = model.Cena;
                    ogloszenie.KategoriaId = model.KategoriaId;
                    ogloszenie.LokalizacjaId = model.LokalizacjaId;
                    ogloszenie.DataWaznosci = model.DataWaznosci;
                    ogloszenie.CzyAktywne = model.CzyAktywne;

                    // Usuń zaznaczone zdjęcia
                    if (model.ZdjeciaDoUsuniecia != null && model.ZdjeciaDoUsuniecia.Any())
                    {
                        var zdjeciaDoUsuniecia = ogloszenie.Zdjecia
                            .Where(z => model.ZdjeciaDoUsuniecia.Contains(z.Id))
                            .ToList();

                        foreach (var zdjecie in zdjeciaDoUsuniecia)
                        {
                            // Usuń plik z dysku
                            var filePath = Path.Combine(_environment.WebRootPath, "uploads", zdjecie.NazwaPliku);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                            
                            _context.Zdjecia.Remove(zdjecie);
                        }
                    }

                    _context.Update(ogloszenie);
                    await _context.SaveChangesAsync();

                    // Dodaj nowe zdjęcia
                    if (model.NoweZdjecia != null && model.NoweZdjecia.Any())
                    {
                        await SaveZdjeciaAsync(ogloszenie.Id, model.NoweZdjecia);
                    }

                    _logger.LogInformation("Zaktualizowano ogłoszenie: {Id} przez użytkownika: {UserId}", id, user?.Id);
                    
                    TempData["SuccessMessage"] = "Ogłoszenie zostało pomyślnie zaktualizowane.";
                    return RedirectToAction(nameof(Details), new { id = ogloszenie.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OgloszenieExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji ogłoszenia {Id}", id);
                    TempData["ErrorMessage"] = "Wystąpił błąd podczas aktualizacji ogłoszenia.";
                }
            }

            ViewBag.Kategorie = await _context.Kategorie.ToListAsync();
            ViewBag.Lokalizacje = await _context.Lokalizacje.ToListAsync();
            return View(model);
        }

        // POST: Asynchroniczne dodawanie odpowiedzi
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddResponse(int ogloszenieId, string tresc)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var odpowiedz = new Answer
                {
                    OgloszenieId = ogloszenieId,
                    Tresc = tresc,
                    AutorId = user!.Id
                };

                _context.Odpowiedzi.Add(odpowiedz);
                await _context.SaveChangesAsync();

                // Zwróć częściowy widok z nową odpowiedzią
                await _context.Entry(odpowiedz)
                    .Reference(o => o.Autor)
                    .LoadAsync();

                return PartialView("_ResponsePartial", odpowiedz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania odpowiedzi");
                return BadRequest("Wystąpił błąd podczas dodawania odpowiedzi");
            }
        }

        // GET: Listing/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ogloszenie = await _context.Ogloszenia
                .Include(o => o.Autor)
                .Include(o => o.Kategoria)
                .Include(o => o.Lokalizacja)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (ogloszenie == null)
            {
                return NotFound();
            }

            // Sprawdź czy użytkownik jest właścicielem ogłoszenia
            var user = await _userManager.GetUserAsync(User);
            if (ogloszenie.AutorId != user?.Id)
            {
                return Forbid();
            }

            return View(ogloszenie);
        }

        // POST: Listing/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var ogloszenie = await _context.Ogloszenia
                    .Include(o => o.Zdjecia)
                    .Include(o => o.Odpowiedzi)
                    .FirstOrDefaultAsync(o => o.Id == id);
                    
                if (ogloszenie == null)
                {
                    return NotFound();
                }

                // Sprawdź czy użytkownik jest właścicielem ogłoszenia
                var user = await _userManager.GetUserAsync(User);
                if (ogloszenie.AutorId != user?.Id)
                {
                    return Forbid();
                }

                // Usuń powiązane zdjęcia z systemu plików
                if (ogloszenie.Zdjecia?.Any() == true)
                {
                    foreach (var zdjecie in ogloszenie.Zdjecia)
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, "uploads", zdjecie.NazwaPliku);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

                // Usuń ogłoszenie (kaskadowo usunie też odpowiedzi i zdjęcia z bazy)
                _context.Ogloszenia.Remove(ogloszenie);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usunięto ogłoszenie: {Id} przez użytkownika: {UserId}", id, user?.Id);
                
                TempData["SuccessMessage"] = "Ogłoszenie zostało pomyślnie usunięte.";
                return RedirectToAction(nameof(MyListings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania ogłoszenia {Id}", id);
                TempData["ErrorMessage"] = "Wystąpił błąd podczas usuwania ogłoszenia.";
                return RedirectToAction(nameof(MyListings));
            }
        }

        // Metody pomocnicze
        private bool OgloszenieExists(int id)
        {
            return _context.Ogloszenia.Any(e => e.Id == id);
        }

        private async Task SaveZdjeciaAsync(int ogloszenieId, List<IFormFile> zdjecia)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            foreach (var zdjecie in zdjecia)
            {
                if (zdjecie.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{zdjecie.FileName}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await zdjecie.CopyToAsync(stream);
                    }

                    var zdjecieModel = new Photo
                    {
                        OgloszenieId = ogloszenieId,
                        NazwaPliku = fileName,
                        RozmiarPliku = zdjecie.Length
                    };

                    _context.Zdjecia.Add(zdjecieModel);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}