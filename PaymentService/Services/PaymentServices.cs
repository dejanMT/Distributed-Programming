using MongoDB.Driver;
using PaymentService.Models;
using System.Text.Json;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

namespace PaymentService.Services
{
    public class PaymentServices 
    {
        private readonly IMongoCollection<Payment> _payments;
        private readonly HttpClient _httpClient;
        private readonly string apiKey;
        private readonly string host;

        public PaymentServices(IMongoCollection<Payment> payments, HttpClient httpClient, IConfiguration config)
        {
            _payments = payments;
            _httpClient = httpClient;
            apiKey = config["TaxiAPI:Key"]!;
            host = config["TaxiAPI:Host"]!;
        }

        public async Task<decimal> GetTaxiFareAsync(decimal startLatitude, decimal startLongitude, decimal endLatitude, decimal endLongitude)
        {
            var requestUrl = $"https://taxi-fare-calculator.p.rapidapi.com/search-geo?dep_lat={startLatitude}&dep_lng={startLongitude}&arr_lat={endLatitude}&arr_lng={endLongitude}";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("X-RapidAPI-Key", apiKey);
            request.Headers.Add("X-RapidAPI-Host", host);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"request failed - status code: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;

            if (!root.TryGetProperty("journey", out var journey))
            {
                throw new Exception("journey is missing from the response");
            }

            if (!journey.TryGetProperty("fares", out var fares) ||  fares.GetArrayLength() == 0 || fares.ValueKind != JsonValueKind.Array)
            {
                throw new Exception("fares is missing from the array");
            }

            var firstFare = fares[0];

            if (!firstFare.TryGetProperty("price_in_cents", out var priceElement) || priceElement.ValueKind != JsonValueKind.Number)
            {
                throw new Exception("'price_in_cents is missing");
            }

            var cents = priceElement.GetInt32();
            return cents / 100m;
        }






        public decimal CalculateTotal(decimal baseFare, string cabType, DateTime time, int passengers, bool discount)
        {

            decimal cabMultiplier;
            if (cabType == "Economic")
            {
                cabMultiplier = 1m;
            }
            else if (cabType == "Premium")
            {
                cabMultiplier = 1.2m;
            }
            else if (cabType == "Executive")
            {
                cabMultiplier = 1.4m;
            }
            else
            {
                throw new ArgumentException("invalid cab selected");
            }

            decimal timeMultiplier;
            if (time.TimeOfDay.Hours >= 0 && time.TimeOfDay.Hours < 8)
            {
                timeMultiplier = 1.2m;
            }
            else
            {
                timeMultiplier = 1m;
            }

            decimal passengerMultiplier;
            if (passengers <= 4)
            {
                passengerMultiplier = 1;
            }
            else if (passengers <= 8)
            {
                passengerMultiplier = 2;
            }
            else
            {
                throw new ArgumentException("passengers amount exceeded");
            }

            decimal discountMultiplier;
            if (discount)
            {
                discountMultiplier = 0.9m;
            }
            else
            {
                discountMultiplier = 1m;
            }

            return baseFare * cabMultiplier * timeMultiplier * passengerMultiplier * discountMultiplier;
        }

        public async Task<Payment> CreatePayment(Payment payment, bool applyDiscount)
        {
            var baseFare = await GetTaxiFareAsync(
                payment.StartLatitude,
                payment.StartLongitude,
                payment.EndLatitude,
                payment.EndLongitude
            );

            payment.BaseFare = baseFare;
            payment.Total = CalculateTotal(baseFare, payment.CabType!, payment.BookingTime, payment.Passengers, applyDiscount);
            payment.DiscountApplied = applyDiscount;

            await _payments.InsertOneAsync(payment);

  
            var pastPayments = await _payments.Find(p => p.UserEmail == payment.UserEmail).ToListAsync();
            var nonDiscountedCount = pastPayments.Count(p => p.DiscountApplied == false);

            if (nonDiscountedCount == 3)
            {
                using var eventClient = new HttpClient();
                var notifyUrl = $"https://localhost:7094/api/Notification/discount?email={payment.UserEmail}";

                try
                {
                    var response = await eventClient.PostAsync(notifyUrl, null);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Notification failed: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error: {ex.Message}");
                }
            }

            return payment;
        }



        public async Task<List<Payment>> GetPayments(string email)
        {
            return await _payments.Find(p => p.UserEmail == email).ToListAsync();
        }


    }
}
