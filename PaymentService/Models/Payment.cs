using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PaymentService.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? UserEmail { get; set; }
        public string? BookingId { get; set; }

        public decimal StartLatitude { get; set; }
        public decimal StartLongitude { get; set; }
        public decimal EndLatitude { get; set; }
        public decimal EndLongitude { get; set; }

        public decimal BaseFare { get; set; }
        public decimal Total { get; set; }

        public string? CabType { get; set; }
        public int Passengers { get; set; }
        public DateTime BookingTime { get; set; }

        public bool DiscountApplied { get; set; }
        public DateTime PaidOn { get; set; } = DateTime.UtcNow;
    }
}
