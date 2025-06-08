using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingService.Models
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? UserEmail { get; set; }
        public string? StartLocation { get; set; }
        public string? EndLocation { get; set; }
        public DateTime BookingDateTime { get; set; }
        public int Passengers { get; set; }
        public string? CabType { get; set; }
        public bool Completed { get; set; } = false;
    }
}
