using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LocationService.Models
{
    public class Location
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? UserEmail { get; set; }
        public string? Name { get; set; } 
        public string? Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
