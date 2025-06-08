using OgloszeniaSytem.Models;

namespace OgloszeniaSytem.Services
{
    public interface ISimilarListingService
    {
        Task<List<SimilarListingDto>> GetSimilarListingsAsync(int listingId, int count = 5);
    }

    public class SimilarListingDto
    {
        public int Id { get; set; }
        public string Tytul { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
        public decimal? Cena { get; set; }
        public DateTime DataPublikacji { get; set; }
        public string AutorNazwa { get; set; } = string.Empty;
        public string KategoriaNazwa { get; set; } = string.Empty;
        public string LokalizacjaNazwa { get; set; } = string.Empty;
        public int LiczbaWyswietlen { get; set; }
        public double SimilarityScore { get; set; }
        public string PierwszeZdjecie { get; set; } = string.Empty;
    }
}