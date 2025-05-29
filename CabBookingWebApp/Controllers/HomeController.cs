using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CabBookingWebApp.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace CabBookingWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _config;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory, IConfiguration config)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _config = config;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(Models.LoginModel model)
    {
        var client = _clientFactory.CreateClient();
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{_config["GatewayApi:BaseUrl"]}/api/UserGateway/login", content);

        if (response.IsSuccessStatusCode)
        {
            HttpContext.Session.SetString("userEmail", model.Email);
            return RedirectToAction("Index", "Notification");
        }

        ViewBag.Error = "Invalid login.";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Signup()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signup(string firstName, string lastName, string email, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ViewData["Error"] = "Passwords do not match.";
            return View();
        }

        var httpClient = _clientFactory.CreateClient();
        var payload = new
        {
            firstName = firstName,
            lastName = lastName,
            email = email,
            password = password
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{_config["GatewayApi:BaseUrl"]}/api/UserGateway/register", content);

        if (response.IsSuccessStatusCode)
        {
            HttpContext.Session.SetString("userEmail", email);
            return RedirectToAction("Index");
        }

        ViewData["Error"] = "Registration failed. Try again.";
        return View();
    }

    [HttpPost("checkWeather")]
    public async Task<IActionResult> CheckWeather(string locationId)
    {
        var client = _clientFactory.CreateClient();
        var response = await client.GetAsync($"{_config["GatewayApi:BaseUrl"]}/api/UserGateway/weather/{locationId}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            ViewBag.WeatherResult = json;
            return View("Index");
        }

        ViewBag.WeatherResult = "Failed to get weather info.";
        return View("Index");
    }



    [HttpPost]
    public async Task<IActionResult> BookCab(Payment model)
    {
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var client = _clientFactory.CreateClient();
        var response = await client.PostAsync($"{_config["GatewayApi:BaseUrl"]}/api/PaymentGateway/create?discount=true", content);

        if (response.IsSuccessStatusCode)
        {
            ViewBag.BookingSuccess = "Cab successfully booked";
            return View("Index");
        }

        ViewBag.BookingError = "Cab booking failed";
        return View("Index");


    }




}
