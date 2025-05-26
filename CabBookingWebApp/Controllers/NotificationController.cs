using CabBookingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CabBookingWebApp.Controllers
{
    public class NotificationController : Controller
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public NotificationController(IHttpClientFactory factory, IConfiguration config)
        {
            _httpClientFactory = factory;
            _config = config;
        }

        public async Task<IActionResult> Index(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Missing email");

            var response = await _httpClientFactory.CreateClient().GetAsync($"{_config["CustomerService:BaseUrl"]}/api/User/{email}/notifications");

            if (!response.IsSuccessStatusCode)
                return Content("Failed to load notifications.");

            var json = await response.Content.ReadAsStringAsync();
            var notifications = JsonSerializer.Deserialize<List<Notification>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(notifications);
        }

    }
}
