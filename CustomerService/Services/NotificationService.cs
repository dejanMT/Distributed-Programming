using CustomerService.Models;
using MongoDB.Driver;

namespace CustomerService.Services
{
    public class NotificationService
    {
        private readonly IMongoCollection<User> _users;

        public NotificationService(IMongoCollection<User> users)
        {
            _users = users;
        }

        public async Task<Notification?> AddDiscountNotification(string userEmail)
        {
            var user = await _users.Find(u => u.Email == userEmail).FirstOrDefaultAsync();
            if (user == null || user.Notifications?.Any(n => n.Message.Contains("discount")) == true)
                return null;

            var notification = new Notification
            {
                Message = "20% discount for your next ride unlocked!",
                Date = DateTime.UtcNow
            };

            var update = Builders<User>.Update.Push(u => u.Notifications, notification);
            await _users.UpdateOneAsync(u => u.Email == userEmail, update);

            return notification;
        }


    }
}
