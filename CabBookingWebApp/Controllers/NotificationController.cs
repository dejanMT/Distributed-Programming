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

        // This pahe will show the notifications for the user
        public async Task<IActionResult> Index()
        {
            // Check if the user is logged in, if not redirect to login page
            var email = HttpContext.Session.GetString("userEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();

            // Set the authorization header if the token is available in session
            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                 new AuthenticationHeaderValue("Bearer", token);


            //var baseUrl = _config["GatewayApi:BaseUrl"];
            var baseUrl = _config["GatewayApiUrl"];
            var response = await client.GetAsync($"{baseUrl}/gateway/customers/{email}/notifications"); // Get notifications for the user from the API

            if (!response.IsSuccessStatusCode)
                return Content("Failed to load notifications.");

            var json = await response.Content.ReadAsStringAsync();

            // If the response is empty, return an empty list
            if (string.IsNullOrWhiteSpace(json))
                return View(new List<Notification>());

            // Deserialize the JSON response to a list of Notification objects
            var notifications = JsonSerializer.Deserialize<List<Notification>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            //return the notifications to the view
            return View(notifications);
        }

    }
}
