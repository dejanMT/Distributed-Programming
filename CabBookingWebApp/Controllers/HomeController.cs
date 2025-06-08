using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CabBookingWebApp.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Globalization;
using System.Net.Http.Headers;

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

    public async Task<IActionResult> Index()
    {
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login");

        var client = _clientFactory.CreateClient();
        //var baseUrl = _config["GatewayApi:BaseUrl"];
        var baseUrl = _config["GatewayApiUrl"];
        var locationsResponse = await client.GetAsync($"{baseUrl}/gateway/locations/all/{email}");
        var locationsJson = await locationsResponse.Content.ReadAsStringAsync();

        var locations = JsonSerializer.Deserialize<List<Location>>(locationsJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });



        var weatherList = new List<LocationWeatherViewModel>();

        foreach (var loc in locations)
        {
            var resp = await client.GetAsync($"{baseUrl}/gateway/locations/weather/{loc.Id}");
            var weatherJson = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(weatherJson);
            var resultString = doc.RootElement.GetProperty("result").GetString()!;

   
            // 1) Get the part after "Temperature: "
            var rawTemp = resultString
                .Split("Temperature: ")[1]
                .Trim(); 


            var numericTempString = rawTemp.TrimEnd('°', 'C', '℃').Trim();


            if (!float.TryParse(numericTempString,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out var tempValue))
            {

                tempValue = 0;
            }

            weatherList.Add(new LocationWeatherViewModel
            {
                LocationName = loc.Name,
                Address = loc.Address,
                Result = resultString,
                Temperature = tempValue
            });
        }


        ViewBag.WeatherList = weatherList;
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

        //var baseUrl = _config["GatewayApi:BaseUrl"];
        var baseUrl = _config["GatewayApiUrl"];
        //var response = await client.PostAsync($"{baseUrl}/api/UserGateway/login", content);
        var response = await client.PostAsync($"{baseUrl}/gateway/customers/login", content);

        if (response.IsSuccessStatusCode)
        {
            //HttpContext.Session.SetString("userEmail", model.Email);
            //return RedirectToAction("Index", "Home");
            // Read the raw JSON/login response
            var loginJson = await response.Content.ReadAsStringAsync();
            var token = loginJson.Trim().Trim('"');
            // Extract token (adjust property name if your service returns something else)
           
             
                     // Store both email and token in session
             HttpContext.Session.SetString("userEmail", model.Email);
             HttpContext.Session.SetString("AuthToken", token);
                     return RedirectToAction("Index", "Home");
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
            surname = lastName,
            email = email,
            password = password
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //var baseUrl = _config["GatewayApi:BaseUrl"];
        var baseUrl = _config["GatewayApiUrl"];
        //var response = await httpClient.PostAsync($"{baseUrl}/api/UserGateway/register", content);
        var response = await httpClient.PostAsync($"{baseUrl}/gateway/customers/register", content);

        if (response.IsSuccessStatusCode)
        {
            HttpContext.Session.SetString("userEmail", email);
            return RedirectToAction("Index");
        }

        ViewData["Error"] = "Registration failed. Try again.";
        return View();
    }

    /// <summary>
    /// Get weather by id (NOT IN USE)
    /// </summary>
    /// <param name="locationId"></param>
    /// <returns></returns>
    public async Task<IActionResult> CheckWeather(string locationId)
    {
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

        //var baseUrl = _config["GatewayApi:BaseUrl"];
        var baseUrl = _config["GatewayApiUrl"];

        // Get weather
        var weatherResponse = await _clientFactory.CreateClient().GetAsync($"{baseUrl}/gateway/weather/{locationId}");
        var weatherJson = await weatherResponse.Content.ReadAsStringAsync();
        var weather = JsonSerializer.Deserialize<WeatherResponse>(weatherJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var locResponse = await _clientFactory.CreateClient().GetAsync($"{baseUrl}/gateway/locations/{locationId}");
        var locationJson = await locResponse.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(locationJson);
        var inner = root.RootElement.GetProperty("result").GetRawText();
        var location = JsonSerializer.Deserialize<Location>(inner, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (weather != null && location != null)
        {
            ViewBag.Weather = new LocationWeatherViewModel
            {
                LocationName = location.Name,
                Address = location.Address,
                Result = weather.Result,
                Temperature = weather.Temperature
            };
        }

        return View("Index");
    }



    [HttpPost]
    public async Task<IActionResult> BookCab(Payment model, bool applyDiscount = false)
    {
        // 1) Generate booking ID
        model.BookingId = Guid.NewGuid().ToString();

        // 2) Prepare HttpClient and attach Bearer token
        var client = _clientFactory.CreateClient();
        var token = HttpContext.Session.GetString("AuthToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // 3) Serialize payload
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // 4) Call the correct Ocelot route: plural + trailing slash
        var baseUrl = _config["GatewayApiUrl"];
        var url = $"{baseUrl}/gateway/payments/?discount={applyDiscount}";
        var response = await client.PostAsync(url, content);

        // 5) Check result as before
        if (response.IsSuccessStatusCode)
        {
            TempData["Message"] = "Cab booked successfully!";
        }
        else
        {
            TempData["Message"] = "Cab booking failed!";
        }

        return RedirectToAction("Index");
    }



    [HttpPost]
    public async Task<IActionResult> AddLocation(Location location)
    {
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login");

        var client = _clientFactory.CreateClient();
        var payload = new
        {
            UserEmail = email,
            name = location.Name,
            address = location.Address,
            latitude = location.Latitude,
            longitude = location.Longitude
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //var baseUrl = _config["GatewayApi:BaseUrl"];
        var baseUrl = _config["GatewayApiUrl"];
        var response = await client.PostAsync($"{baseUrl}/gateway/locations", content);

        if (response.IsSuccessStatusCode)
            TempData["Message"] = "Location saved successfully.";
        else
            TempData["Error"] = "Failed to save location.";

        return RedirectToAction("Index");
    }





}
