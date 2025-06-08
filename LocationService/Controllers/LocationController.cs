using LocationService.Models;
using LocationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace LocationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly Services.LocationService _services;
        private readonly WeatherService _weather;

        public LocationController(Services.LocationService services, WeatherService weather)
        {
            _services = services;
            _weather = weather;
        }

        [HttpPost]
        public async Task<IActionResult> AddLocation([FromBody] Location loc)
        {
            var result = await _services.AddLocation(loc);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocationById(string id)
        {
            var result = await _services.GetLocationById(id);
            return Ok(new { id, result });
        }

        [HttpGet("all/{email}")]
        public async Task<IActionResult> GetAllLocationsByEmail(string email)
        {
            var results = await _services.GetAllLocationsByEmail(email);
            return Ok(results);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateLocation(string id, [FromBody] Location loc)
        {
            var result = await _services.UpdateLocation(id, loc);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLocation(string id)
        {
            var success = await _services.DeleteLocation(id);
            return success ? Ok() : NotFound();
        }

        [HttpGet("weather/{id}")]
        public async Task<IActionResult> GetWeather(string id)
        {
            var loc = await _services.GetLocationById(id);
            if (loc == null)
                return NotFound();

            try
            {
                var result = await _weather.GetWeatherAsync(loc.Latitude, loc.Longitude);
                return Ok(new { result });
            }
            catch (HttpRequestException ex)
            {
                // Log ex here
                return StatusCode(502, "Weather lookup failed");
            }
        }


    }
}
