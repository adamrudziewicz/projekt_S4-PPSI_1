using System.Text.Json;
using System.Text.Json.Serialization;

namespace OgloszeniaSytem.Services
{
    public interface IApiService
    {
        Task<WeatherData?> GetWeatherAsync(string city);
        Task<List<string>> GetCitySuggestionsAsync(string query);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<WeatherData?> GetWeatherAsync(string city)
        {
            try
            {
                var apiKey = _configuration["WeatherApi:ApiKey"];
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Brak klucza API dla OpenWeatherMap");
                    return null;
                }

                var encodedCity = Uri.EscapeDataString(city);
                var url = $"https://api.openweathermap.org/data/2.5/weather?q={encodedCity}&appid={apiKey}&units=metric&lang=pl";
                
                _logger.LogInformation("Wysyłanie zapytania do: {Url}", url);
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Błąd API pogody. Status: {StatusCode}, Odpowiedź: {Content}", 
                        response.StatusCode, errorContent);
                    return null;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Odpowiedź z API: {Content}", jsonContent);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    PropertyNameCaseInsensitive = true
                };

                var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(jsonContent, options);
                
                if (weatherData == null)
                {
                    _logger.LogError("Nie udało się zdeserializować odpowiedzi z API pogody");
                    return null;
                }
                
                return new WeatherData
                {
                    Name = weatherData.Name,
                    Temperature = weatherData.Main.Temp,
                    FeelsLike = weatherData.Main.FeelsLike,
                    Description = weatherData.Weather.FirstOrDefault()?.Description ?? "",
                    Main = weatherData.Weather.FirstOrDefault()?.Main ?? "",
                    Humidity = weatherData.Main.Humidity,
                    Pressure = weatherData.Main.Pressure
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Błąd połączenia podczas pobierania danych pogodowych dla miasta: {City}", city);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Błąd deserializacji JSON dla miasta: {City}", city);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nieoczekiwany błąd podczas pobierania danych pogodowych dla miasta: {City}", city);
                return null;
            }
        }

        public async Task<List<string>> GetCitySuggestionsAsync(string query)
        {
            try
            {
                var polishCities = new List<string>
                {
                    "Warszawa", "Kraków", "Gdańsk", "Wrocław", "Poznań", "Łódź", 
                    "Szczecin", "Bydgoszcz", "Lublin", "Katowice", "Białystok", 
                    "Gdynia", "Częstochowa", "Radom", "Sosnowiec", "Toruń"
                };

                return polishCities
                    .Where(city => city.ToLower().Contains(query.ToLower()))
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas pobierania sugestii miast dla zapytania: {Query}", query);
                return new List<string>();
            }
        }
    }
    
    public class OpenWeatherMapResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("main")]
        public MainData Main { get; set; } = new();

        [JsonPropertyName("weather")]
        public List<WeatherInfo> Weather { get; set; } = new();
    }

    public class MainData
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }

        [JsonPropertyName("pressure")]
        public int Pressure { get; set; }
    }

    public class WeatherInfo
    {
        [JsonPropertyName("main")]
        public string Main { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
    
    public class WeatherData
    {
        public string Name { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Main { get; set; } = string.Empty;
        public int Humidity { get; set; }
        public int Pressure { get; set; }
    }
}