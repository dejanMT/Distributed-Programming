using Middleware;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CustomerService.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? Surname { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string? Password { get; set; }
        public List<Notification>? Notifications { get; set; }

        public string? Salt { get; set; }

        public void SetPassword(string password, IEncryptor encryptor)
        {
            Salt = encryptor.GetSalt();
            Password = encryptor.GetHash(password, Salt);
        }

        public bool ValidatePassword(string password, IEncryptor encryptor)
        {
            return Password == encryptor.GetHash(password, Salt);
        }
    }
}
