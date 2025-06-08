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

    // Loads the home page with all teh neccessary data
    public async Task<IActionResult> Index()
    {
        // Check if user is logged in if not direst to login page
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Home");

        var client = _clientFactory.CreateClient();
        //var baseUrl = _config["GatewayApi:BaseUrl"];
        var baseUrl = _config["GatewayApiUrl"];
        var locationsResponse = await client.GetAsync($"{baseUrl}/gateway/locations/all/{email}"); // get all locations for the user with the given email
        var locationsJson = await locationsResponse.Content.ReadAsStringAsync();

        // Deserialize the JSON response into a list of Location objects
        var locations = JsonSerializer.Deserialize<List<Location>>(locationsJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });



        var weatherList = new List<LocationWeatherViewModel>();

        foreach (var loc in locations)
        {
            var resp = await client.GetAsync($"{baseUrl}/gateway/locations/weather/{loc.Id}"); // get weather for the location with the given location id
            var weatherJson = await resp.Content.ReadAsStringAsync();

            // Parse the JSON response to extract the weather information
            using var doc = JsonDocument.Parse(weatherJson);
            var resultString = doc.RootElement.GetProperty("result").GetString()!;
            // Extract the temperature from the result string
            var rawTemp = resultString
                .Split("Temperature: ")[1]
                .Trim();

            // Remove the degree symbol and 'C' from the temperature string
            var numericTempString = rawTemp.TrimEnd('°', 'C', '℃').Trim();

            // Try to parse the numeric temperature string into a float
            if (!float.TryParse(numericTempString,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out var tempValue))
            {

                tempValue = 0;
            }

            // Create a new LocationWeatherViewModel object and populate it with the location and weather data
            weatherList.Add(new LocationWeatherViewModel
            {
                LocationId = loc.Id,
                LocationName = loc.Name,
                Address = loc.Address,
                Result = resultString,
                Temperature = tempValue,
                Longitude = loc.Longitude,
                Latitude = loc.Latitude
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

    // Login action to handle user login
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
        return RedirectToAction("Login", "Home");
    }

    [HttpGet]
    public IActionResult Signup()
    {
        return View();
    }

    // Signup action to handles new user registration
    [HttpPost]
    public async Task<IActionResult> Signup(string firstName, string lastName, string email, string password, string confirmPassword)
    {
        //This checks if teh password and confirm password matcheSd 
        if (password != confirmPassword)
        {
            ViewData["Error"] = "Passwords do not match.";
            return View();
        }

        var httpClient = _clientFactory.CreateClient();
        // Create the payload for registration
        var payload = new
        {
            firstName = firstName,
            surname = lastName,
            email = email,
            password = password
        };

        // Serialize the payload to JSON
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

    //This action handles the cab booking process
    [HttpPost]
    public async Task<IActionResult> BookCab(Payment model, bool applyDiscount = false)
    {
        // Auto Generate booking ID
        model.BookingId = Guid.NewGuid().ToString();

        // Prepare HttpClient and attach Bearer token
        var client = _clientFactory.CreateClient();
        var token = HttpContext.Session.GetString("AuthToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // Serialize payload
        var json = JsonSerializer.Serialize(model);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Call the correct Ocelot route: plural + trailing slash
        var baseUrl = _config["GatewayApiUrl"];
        var url = $"{baseUrl}/gateway/payments/?discount={applyDiscount}";
        var response = await client.PostAsync(url, content);

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


    //This action handles adding a new favourite location
    [HttpPost]
    public async Task<IActionResult> AddLocation(Location location)
    {
        // Check if user is logged in, if not redirect to login page
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Home");

        var client = _clientFactory.CreateClient();
        var payload = new // Create a payload with the location details and user email  
        {
            UserEmail = email,
            name = location.Name,
            address = location.Address,
            latitude = location.Latitude,
            longitude = location.Longitude
        };

        // Serialize the payload to JSON    
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //var baseUrl = _config["GatewayApi:BaseUrl"];
        var baseUrl = _config["GatewayApiUrl"];
        var response = await client.PostAsync($"{baseUrl}/gateway/locations", content); // Post the new location to the API

        if (response.IsSuccessStatusCode)
            TempData["Message"] = "Location saved successfully.";
        else
            TempData["Error"] = "Failed to save location.";

        return RedirectToAction("Index");
    }

    //This action handles deleting of a favourite location
    [HttpPost]
    public async Task<IActionResult> DeleteLocation(string id)
    {
        // Check if user is logged in, if not redirect to login page
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Home");

        var client = _clientFactory.CreateClient();
        var baseUrl = _config["GatewayApiUrl"];
        var response = await client.DeleteAsync($"{baseUrl}/gateway/locations/delete/{id}"); // Delete the location with the given id

        TempData["Message"] = response.IsSuccessStatusCode ? "Location deleted successfully." : "Failed to delete location.";
        return RedirectToAction("Index");
    }

    //This action handles updating of a favourite location
    [HttpPost]
    public async Task<IActionResult> UpdateLocation(Location location)
    {
        // Check if user is logged in, if not redirect to login page
        var email = HttpContext.Session.GetString("userEmail");
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login", "Home");


        var client = _clientFactory.CreateClient();
        var payload = new // Create a payload with the updated location details and user email
        {
            Id = location.Id,
            UserEmail = email,
            Name = location.Name,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var baseUrl = _config["GatewayApiUrl"];
        var response = await client.PutAsync($"{baseUrl}/gateway/locations/update/{location.Id}", content); // Update the location with the given id

        TempData["Message"] = response.IsSuccessStatusCode ? "Location updated successfully." : "Failed to update location.";
        return RedirectToAction("Index");
    }



}
