using CustomerService.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CustomerService.Controllers
{

    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {

        private readonly IMongoCollection<User> _users;

        public NotificationController(IMongoCollection<User> users)
        {
            _users = users;
        }

        // This endpoint retrieves all notifications for a user by their email.
        [HttpGet("{email}/notifications")]
        public async Task<IActionResult> GetNotifications(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync(); // Find user by email
            if (user == null)
                return NotFound("User not found");

            return Ok(user.Notifications ?? new List<Notification>());
        }

        // This endpoint allows adding a cab-ready notification for a user by their email.
        [HttpPost("cabready")]
        public async Task<IActionResult> NotifyCabReady(string email, [FromBody] string rideMessage)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
                return NotFound("User not found");

            var notification = new Notification
            {
                Message = rideMessage,
                Date = DateTime.UtcNow
            };

            var update = Builders<User>.Update.Push(u => u.Notifications, notification);
            await _users.UpdateOneAsync(u => u.Email == email, update);

            return Ok("Cab-ready notification added.");
        }

        // This endpoint allows adding a discount notification for a user by their email.
        [HttpPost("discount")]
        public async Task<IActionResult> NotifyDiscount(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
                return NotFound("User not found");

            var notification = new Notification
            {
                Message = "Dicsount Unlocked!",
                Date = DateTime.UtcNow
            };

            var update = Builders<User>.Update.Push(u => u.Notifications, notification);
            await _users.UpdateOneAsync(u => u.Email == email, update);

            return Ok("Discount notification added.");
        }


    }
}
