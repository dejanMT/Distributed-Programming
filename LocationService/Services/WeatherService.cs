using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LocationService.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        private readonly string apiKey;
        private readonly string host;

        public WeatherService(HttpClient httpClient, IConfiguration config)
        {
            //_httpClient = new HttpClient();
            _httpClient = httpClient;
            apiKey = config["WeatherAPI-Key"]!;
            host = config["WeatherAPI-Host"]!;
        }

        public async Task<string> GetWeatherAsync(decimal lat, decimal lng)
        {
            //with locality name: https://weatherapi-com.p.rapidapi.com/current.json?q=Zejtun
            //with cords. https://weatherapi-com.p.rapidapi.com/current.json?q=35.8771%2C14.5406
            
            var requestUrl = $"https://weatherapi-com.p.rapidapi.com/current.json?q={lat}%2C{lng}";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("X-RapidAPI-Key", apiKey);
            request.Headers.Add("X-RapidAPI-Host", host);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            var condition = root.GetProperty("current").GetProperty("condition").GetProperty("text").GetString();
            var temp = root.GetProperty("current").GetProperty("temp_c").GetDecimal();

            return $"Weather: {condition}, Temperature: {temp}°C";
        }
    }
}
