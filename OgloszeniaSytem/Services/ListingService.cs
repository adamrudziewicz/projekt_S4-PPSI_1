using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OgloszeniaSytem.Data;
using OgloszeniaSytem.Models;
using OgloszeniaSytem.ViewModels;

namespace OgloszeniaSytem.Services
{
    public class ListingService : IListingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ListingService> _logger;
        private readonly IWebHostEnvironment _environment;

        public ListingService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<ListingService> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _environment = environment;
        }
        private bool CheckIfImageExists(string fileName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            return File.Exists(filePath);
        }
        public async Task<PaginatedList<Listing>> GetOgloszeniaAsync(string? kategoria, string? lokalizacja, string? search, int page)
        {
            const int pageSize = 10;
            var cacheKey = $"ogloszenia_{kategoria}_{lokalizacja}_{search}_{page}";

            if (_cache.TryGetValue(cacheKey, out PaginatedList<Listing>? cachedResult))
            {
                return cachedResult!;
            }

            var query = _context.Ogloszenia
                .Include(o => o.Autor)
                .Include(o => o.Kategoria)
                .Include(o => o.Lokalizacja)
                .Where(o => o.CzyAktywne);

            if (!string.IsNullOrEmpty(kategoria))
            {
                query = query.Where(o => o.Kategoria.Nazwa == kategoria);
            }

            if (!string.IsNullOrEmpty(lokalizacja))
            {
                query = query.Where(o => o.Lokalizacja.Nazwa == lokalizacja);
            }

            // NOWA FUNKCJONALNOŚĆ WYSZUKIWANIA
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(o => 
                    o.Tytul.ToLower().Contains(searchLower) || 
                    o.Opis.ToLower().Contains(searchLower) ||
                    o.Kategoria.Nazwa.ToLower().Contains(searchLower));
            }

            query = query.OrderByDescending(o => o.DataPublikacji);

            var result = await PaginatedList<Listing>.CreateAsync(query, page, pageSize);

            // Cache na 5 minut
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<Listing?> GetOgloszenieByIdAsync(int id)
        {
            return await _context.Ogloszenia
                .Include(o => o.Autor)
                .Include(o => o.Kategoria)
                .Include(o => o.Lokalizacja)
                .Include(o => o.Zdjecia)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task CreateOgloszenieAsync(Listing listing, List<IFormFile>? zdjecia)
        {
            _context.Ogloszenia.Add(listing);
            await _context.SaveChangesAsync();

            if (zdjecia != null && zdjecia.Any())
            {
                await SaveZdjeciaAsync(listing.Id, zdjecia);
            }

            // Wyczyść cache
            _cache.Remove($"ogloszenia__{1}");
        }

        public async Task IncreaseViewCountAsync(int id)
        {
            var ogloszenie = await _context.Ogloszenia.FindAsync(id);
            if (ogloszenie != null)
            {
                ogloszenie.LiczbaWyswietlen++;
                await _context.SaveChangesAsync();
            }
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