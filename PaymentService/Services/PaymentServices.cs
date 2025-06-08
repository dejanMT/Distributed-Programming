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

        // Get the estimated taxi fare from the FareService
        public async Task<decimal> GetTaxiFareAsync(decimal startLatitude, decimal startLongitude, decimal endLatitude, decimal endLongitude)
        {
            // Use the FareService API to get the estimated fare
            var requestUrl = $"https://fare-service-521568789858.europe-west1.run.app/api/Fare/estimate?startLatitude={startLatitude}&startLongitude={startLongitude}&endLatitude={endLatitude}&endLongitude={endLongitude}";

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"FareService request failed - status code: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(jsonResponse);
            var fare = document.RootElement.GetProperty("fare").GetDecimal();

            return fare;
        }


        // Calculate the total fare based on various parameters
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

        // Create a new payment and calculate the total fare
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
            // Notify the user that their payment was successful after 3 minutes
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(3));

                var rideInfo = $"Your cab is waiting for you. Pickup: ({payment.StartLatitude}, {payment.StartLongitude}) → Drop-off: ({payment.EndLatitude}, {payment.EndLongitude}), Reference No. {payment.BookingId}";
                var notifyUrl = $"https://customer-service-521568789858.europe-west1.run.app/api/Notification/cabready?email={payment.UserEmail}";

                // Send the notification to the customer service
                try
                {
                    using var client = new HttpClient();
                    var content = new StringContent($"\"{rideInfo}\"", System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(notifyUrl, content);
                    Console.WriteLine($"cab notification sent: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });


            // Check if the user is eligible for a discount after 3 non-discounted bookings
            var pastBookings = await _payments.Find(p => p.UserEmail == payment.UserEmail).ToListAsync();
            var nonDiscountedCount = pastBookings.Count(p => p.DiscountApplied == false);

            if (nonDiscountedCount == 3)
            {
                using var eventClient = new HttpClient();
                var notifyUrl = $"https://customer-service-521568789858.europe-west1.run.app/api/Notification/discount?email={payment.UserEmail}";

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


        // Get all payments for a specific user by email
        public async Task<List<Payment>> GetPayments(string email)
        {
            return await _payments.Find(p => p.UserEmail == email).ToListAsync();
        }


    }
}
