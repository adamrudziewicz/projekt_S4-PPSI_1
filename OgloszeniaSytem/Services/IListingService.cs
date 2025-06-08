using OgloszeniaSytem.Models;
using OgloszeniaSytem.ViewModels;

namespace OgloszeniaSytem.Services
{
    public interface IListingService
    {
        Task<PaginatedList<Listing>> GetOgloszeniaAsync(string? kategoria, string? lokalizacja, string? search, int page);
        Task<Listing?> GetOgloszenieByIdAsync(int id);
        Task CreateOgloszenieAsync(Listing listing, List<IFormFile>? zdjecia);
        Task IncreaseViewCountAsync(int id);
    }
}