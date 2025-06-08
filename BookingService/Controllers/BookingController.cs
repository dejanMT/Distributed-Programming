using BookingService.Models;
using BookingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService.Services.BookingService _bookingService;

        public BookingController(BookingService.Services.BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // This endpoint is used to create a new booking
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
        {
            var result = await _bookingService.CreateBooking(booking);
            return Ok(result);
        }

        // This endpoint is used to get current bookings for a user
        [HttpGet("current/{email}")]
        public async Task<IActionResult> GetCurrentBookings(string email)
        {
            var bookings = await _bookingService.GetCurrentBookings(email);
            return Ok(bookings);
        }

        // This endpoint is used to mark a booking as completed
        [HttpGet("past/{email}")]
        public async Task<IActionResult> GetPastBookings(string email)
        {
            var bookings = await _bookingService.GetPastBookings(email);
            return Ok(bookings);
        }
    }
}
