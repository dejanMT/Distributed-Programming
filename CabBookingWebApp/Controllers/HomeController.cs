using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CabBookingWebApp.Models;
using System.Text.Json;
using System.Text;

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
        var baseUrl = _config["GatewayApi:BaseUrl"];
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{baseUrl}/api/UserGateway/login", content);

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
}
