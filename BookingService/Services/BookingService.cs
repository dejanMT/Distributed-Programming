using BookingService.Models;
using MongoDB.Driver;

namespace BookingService.Services
{
    public class BookingService
    {
        private readonly IMongoCollection<Booking> _bookings;

        public BookingService(IMongoCollection<Booking> bookings)
        {
            _bookings = bookings;
        }

        public async Task<Booking> CreateBooking(Booking booking)
        {
            await _bookings.InsertOneAsync(booking);
            return booking;
        }

        public async Task<List<Booking>> GetCurrentBookings(string email)
        {
            return await _bookings.Find(b => b.UserEmail == email && !b.Completed).ToListAsync();
        }

        public async Task<List<Booking>> GetPastBookings(string email)
        {
            return await _bookings.Find(b => b.UserEmail == email && b.Completed).ToListAsync();
        }

        public async Task BookingComplete(string id)
        {
            await _bookings.UpdateOneAsync(Builders<Booking>.Filter.Eq(b => b.Id, id), Builders<Booking>.Update.Set(b => b.Completed, true));
        }
    }
}
