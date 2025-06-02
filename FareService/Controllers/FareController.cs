using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FareService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FareController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public FareController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet("estimate")]
        public async Task<IActionResult> GetFare(decimal startLatitude, decimal startLongitude, decimal endLatitude, decimal endLongitude)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _config["TaxiAPI:Key"]);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _config["TaxiAPI:Host"]);

            var url = $"https://taxi-fare-calculator.p.rapidapi.com/search-geo?dep_lat={startLatitude}&dep_lng={startLongitude}&arr_lat={endLatitude}&arr_lng={endLongitude}";

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return BadRequest("Failed to get fare from API");

            var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (!root.TryGetProperty("journey", out var journey) ||
                !journey.TryGetProperty("fares", out var fares) ||
                fares.GetArrayLength() == 0 ||
                fares[0].TryGetProperty("price_in_cents", out var priceElement) == false)
            {
                return BadRequest("Invalid response structure");
            }

            var priceInCents = priceElement.GetInt32();
            decimal priceInEuros = priceInCents / 100m;

            return Ok(new { fare = priceInEuros });
        }
    }
}
