using Microsoft.EntityFrameworkCore;
using OgloszeniaSytem.Data;
using OgloszeniaSytem.Models;
using System.Text.RegularExpressions;

namespace OgloszeniaSytem.Services
{
    public class SimilarListingService : ISimilarListingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SimilarListingService> _logger;

        public SimilarListingService(ApplicationDbContext context, ILogger<SimilarListingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SimilarListingDto>> GetSimilarListingsAsync(int listingId, int count = 5)
        {
            try
            {
                // Pobierz główne ogłoszenie
                var mainListing = await _context.Ogloszenia
                    .Include(l => l.Kategoria)
                    .Include(l => l.Lokalizacja)
                    .Include(l => l.Autor)
                    .Include(l => l.Zdjecia)
                    .FirstOrDefaultAsync(l => l.Id == listingId && l.CzyAktywne);

                if (mainListing == null)
                {
                    _logger.LogWarning("Nie znaleziono ogłoszenia o ID: {ListingId}", listingId);
                    return new List<SimilarListingDto>();
                }

                // Pobierz wszystkie aktywne ogłoszenia oprócz głównego
                var allListings = await _context.Ogloszenia
                    .Include(l => l.Kategoria)
                    .Include(l => l.Lokalizacja)
                    .Include(l => l.Autor)
                    .Include(l => l.Zdjecia)
                    .Where(l => l.Id != listingId && l.CzyAktywne)
                    .ToListAsync();

                // Oblicz podobieństwo dla każdego ogłoszenia
                var similarListings = allListings
                    .Select(listing => new SimilarListingDto
                    {
                        Id = listing.Id,
                        Tytul = listing.Tytul,
                        Opis = TruncateText(listing.Opis, 150),
                        Cena = listing.Cena,
                        DataPublikacji = listing.DataPublikacji,
                        AutorNazwa = listing.Autor?.UserName ?? "Nieznany",
                        KategoriaNazwa = listing.Kategoria?.Nazwa ?? "Bez kategorii",
                        LokalizacjaNazwa = listing.Lokalizacja?.Nazwa ?? "Nieznana",
                        LiczbaWyswietlen = listing.LiczbaWyswietlen,
                        PierwszeZdjecie = listing.Zdjecia.FirstOrDefault()?.NazwaPliku ?? "",
                        SimilarityScore = CalculateSimilarity(mainListing, listing)
                    })
                    .Where(dto => dto.SimilarityScore > 0)
                    .OrderByDescending(dto => dto.SimilarityScore)
                    .Take(count)
                    .ToList();

                _logger.LogInformation("Znaleziono {Count} podobnych ogłoszeń dla ID: {ListingId}", 
                    similarListings.Count, listingId);

                return similarListings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wyszukiwania podobnych ogłoszeń dla ID: {ListingId}", listingId);
                return new List<SimilarListingDto>();
            }
        }

        private double CalculateSimilarity(Listing mainListing, Listing compareListing)
        {
            double score = 0;

            // 1. Podobieństwo kategorii (najważniejsze - 40%)
            if (mainListing.KategoriaId == compareListing.KategoriaId)
            {
                score += 40;
            }

            // 2. Podobieństwo lokalizacji (30%)
            if (mainListing.LokalizacjaId == compareListing.LokalizacjaId)
            {
                score += 30;
            }

            // 3. Podobieństwo ceny (15%)
            if (mainListing.Cena.HasValue && compareListing.Cena.HasValue)
            {
                var priceDifference = Math.Abs((double)(mainListing.Cena.Value - compareListing.Cena.Value));
                var avgPrice = (double)(mainListing.Cena.Value + compareListing.Cena.Value) / 2;
                
                if (avgPrice > 0)
                {
                    var priceRatio = 1 - (priceDifference / avgPrice);
                    score += Math.Max(0, priceRatio * 15);
                }
            }
            else if (!mainListing.Cena.HasValue && !compareListing.Cena.HasValue)
            {
                // Jeśli oba nie mają ceny, daj małe podobieństwo
                score += 5;
            }

            // 4. Podobieństwo słów kluczowych w tytule (15%)
            var titleSimilarity = CalculateTextSimilarity(mainListing.Tytul, compareListing.Tytul);
            score += titleSimilarity * 15;

            return Math.Round(score, 2);
        }

        private double CalculateTextSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;

            var words1 = ExtractWords(text1.ToLower());
            var words2 = ExtractWords(text2.ToLower());

            if (!words1.Any() || !words2.Any())
                return 0;

            var commonWords = words1.Intersect(words2).Count();
            var totalWords = words1.Union(words2).Count();

            return totalWords > 0 ? (double)commonWords / totalWords : 0;
        }

        private List<string> ExtractWords(string text)
        {
            // Usuń znaki interpunkcyjne i podziel na słowa
            var cleanText = Regex.Replace(text, @"[^\w\s]", " ");
            return cleanText.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                          .Where(word => word.Length > 2) // Ignoruj słowa krótsze niż 3 znaki
                          .ToList();
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }
    }
}