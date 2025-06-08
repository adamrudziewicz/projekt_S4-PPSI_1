using Microsoft.AspNetCore.Mvc;
using OgloszeniaSytem.Services;

namespace OgloszeniaSytem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IApiService _apiService;
        private readonly IListingService _listingService;
        private readonly ISimilarListingService _similarListingService;

        public ApiController(IApiService apiService, IListingService listingService, ISimilarListingService similarListingService)
        {
            _apiService = apiService;
            _listingService = listingService;
            _similarListingService = similarListingService;
        }
        
        [HttpGet("weather/{city}")]
        public async Task<IActionResult> GetWeather(string city)
        {
            var weather = await _apiService.GetWeatherAsync(city);
            if (weather == null)
                return NotFound();

            return Ok(weather);
        }

        [HttpGet("cities/suggestions")]
        public async Task<IActionResult> GetCitySuggestions([FromQuery] string query)
        {
            var suggestions = await _apiService.GetCitySuggestionsAsync(query);
            return Ok(suggestions);
        }

        [HttpGet("listing/similar/{id}")]
        public async Task<IActionResult> GetSimilarOgloszenia(int id, [FromQuery] int count = 5)
        {
            var similarListings = await _similarListingService.GetSimilarListingsAsync(id, count);
            return Ok(similarListings);
        }

        [HttpGet("listing/similar/{id}/detailed")]
        public async Task<IActionResult> GetSimilarOgloszeniaDetailed(int id, [FromQuery] int count = 5)
        {
            var similarListings = await _similarListingService.GetSimilarListingsAsync(id, count);
            
            var result = new
            {
                MainListingId = id,
                Count = similarListings.Count,
                Timestamp = DateTime.UtcNow,
                SimilarListings = similarListings
            };

            return Ok(result);
        }
    }
}