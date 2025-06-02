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

        [HttpGet("{email}/notifications")]
        public async Task<IActionResult> GetNotifications(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
                return NotFound("User not found");

            return Ok(user.Notifications ?? new List<Notification>());
        }

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

    }
}
