using CabBookingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace GatewayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserGatewayController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserGatewayController(IHttpClientFactory factory)
        {
            _httpClientFactory = factory;
        }

        [HttpGet("{email}/notifications")]
        public async Task<IActionResult> GetNotifications(string email)
        {
            //var response = await _httpClientFactory.CreateClient("Customer").GetAsync($"/api/User/{email}/notifications");
            var response = await _httpClientFactory.CreateClient("Customer").GetAsync($"/api/Notification/{email}/notifications");

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement loginData)
        {
            var client = _httpClientFactory.CreateClient("Customer");
            var response = await client.PostAsync("/api/User/login", new StringContent(
                loginData.GetRawText(),
                System.Text.Encoding.UTF8,
                "application/json"
            ));

            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, content);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] JsonElement registerData)
        {
            var client = _httpClientFactory.CreateClient("Customer");
            var response = await client.PostAsync("/api/User/register", new StringContent(
                registerData.GetRawText(),
                System.Text.Encoding.UTF8,
                "application/json"
            ));

            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, content);
        }

        //[HttpPost("book")]
        //public async Task<IActionResult> Book([FromBody] JsonElement booking)
        //{
        //    var client = _httpClientFactory.CreateClient("Payment");
        //    var response = await client.PostAsync("/api/Payment?applyDiscount=true", new StringContent(
        //        booking.GetRawText(),
        //        Encoding.UTF8,
        //        "application/json"));

        //    var content = await response.Content.ReadAsStringAsync();
        //    return StatusCode((int)response.StatusCode, content);
        //}

        //[HttpPost("payment")]
        //public async Task<IActionResult> CreatePayment([FromQuery] bool discount, [FromBody] JsonElement payload)
        //{
        //    var client = _httpClientFactory.CreateClient("Payment");
        //    var content = new StringContent(payload.GetRawText(), Encoding.UTF8, "application/json");
        //    var response = await client.PostAsync($"/api/Payment?discount={discount}", content);
        //    var result = await response.Content.ReadAsStringAsync();
        //    return StatusCode((int)response.StatusCode, result);
        //}

        [HttpPost("payment")]
        public async Task<IActionResult> CreatePayment([FromQuery] bool discount, [FromBody] JsonElement payload)
        {
            var client = _httpClientFactory.CreateClient("Payment");

            var payment = JsonSerializer.Deserialize<Payment>(payload.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var normalizedJson = JsonSerializer.Serialize(payment, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(normalizedJson, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"/api/payment?discount={discount}", content);
            var result = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, result);
        }


        [HttpGet("weather/{id}")]
        public async Task<IActionResult> GetWeather(string id)
        {
            var client = _httpClientFactory.CreateClient("Location");
            var response = await client.GetAsync($"/api/Location/weather/{id}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("location/{id}")]
        public async Task<IActionResult> GetLocationById(string id)
        {
            var client = _httpClientFactory.CreateClient("Location");
            var response = await client.GetAsync($"/api/Location/{id}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("locations/{email}")]
        public async Task<IActionResult> GetLocations(string email)
        {
            var client = _httpClientFactory.CreateClient("Location"); 
            var response = await client.GetAsync($"/api/Location/all/{email}");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }



    }
}
