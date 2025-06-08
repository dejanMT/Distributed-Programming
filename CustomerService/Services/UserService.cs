using CustomerService.Data;
using CustomerService.Models;
using Microsoft.EntityFrameworkCore;
using Middleware;
using MongoDB.Driver;

namespace CustomerService.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IEncryptor _encryptor;

        public UserService(IMongoCollection<User> users, IEncryptor encryptor)
        {
            _users = users;
            _encryptor = encryptor;
        }

        // This is the endpoint for registering a new user
        public async Task<User?> Register(User user)
        {
            var existing = await _users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (existing != null) return null;

            user.SetPassword(user.Password!, _encryptor);

            await _users.InsertOneAsync(user);
            return user;
        }

        // This is the endpoint for logging in a user
        public async Task<User?> Login(string email, string password)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null) return null;

            return user.ValidatePassword(password, _encryptor) ? user : null;
        }

        // This is the endpoint for getting user details by email
        public async Task<User?> GetUserByEmail(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        // This is the endpoint for getting user details by email, excluding the password
        public async Task<User?> GetUserDetails(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null) return null;
            user.Password = null;
            return user;
        }

    }
}
