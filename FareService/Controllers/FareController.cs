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

        // This is responsible for calculating the fare based on the start and end coordinates using the 3rd party api for taxi fare estimation 
        [HttpGet("estimate")]
        public async Task<IActionResult> GetFare(decimal startLatitude, decimal startLongitude, decimal endLatitude, decimal endLongitude)
        {
            var client = _httpClientFactory.CreateClient();
            //ghal testing
            //client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _config["TaxiAPI:Key"]);
            //client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _config["TaxiAPI:Host"]);
            
            client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _config["TaxiAPI-Key"]);
            client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _config["TaxiAPI-Host"]);

            // This is the 3rd party API endpoint for taxi fare estimation
            var url = $"https://taxi-fare-calculator.p.rapidapi.com/search-geo?dep_lat={startLatitude}&dep_lng={startLongitude}&arr_lat={endLatitude}&arr_lng={endLongitude}";

            // Make the API call to get the fare estimate
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return BadRequest("Failed to get fare from API");

            var document = JsonDocument.Parse(content); // Parse the JSON response
            var root = document.RootElement; // Get the root element of the JSON document

            // Check if the response contains the expected structure
            if (!root.TryGetProperty("journey", out var journey) ||
                !journey.TryGetProperty("fares", out var fares) ||
                fares.GetArrayLength() == 0 ||
                fares[0].TryGetProperty("price_in_cents", out var priceElement) == false)
            {
                return BadRequest("Invalid response structure");
            }

            var priceInCents = priceElement.GetInt32(); // Extract the price in cents from the JSON response
            decimal priceInEuros = priceInCents / 100m; // Convert cents to euros

            return Ok(new { fare = priceInEuros }); // Return the fare in euros
        }
    }
}
