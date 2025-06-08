using CabBookingWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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


        public async Task<IActionResult> Index()
        {
            var email = HttpContext.Session.GetString("userEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();


            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                 new AuthenticationHeaderValue("Bearer", token);


            //var baseUrl = _config["GatewayApi:BaseUrl"];
            var baseUrl = _config["GatewayApiUrl"];
            var response = await client.GetAsync($"{baseUrl}/gateway/customers/{email}/notifications");

            if (!response.IsSuccessStatusCode)
                return Content("Failed to load notifications.");

            var json = await response.Content.ReadAsStringAsync();


            if (string.IsNullOrWhiteSpace(json))
                return View(new List<Notification>());

            var notifications = JsonSerializer.Deserialize<List<Notification>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });


            return View(notifications);
        }

    }
}
