namespace CabBookingWebApp.Models
{
    public class Payment
    {
        public string? UserEmail { get; set; }
        public string? BookingId { get; set; }
        public decimal StartLatitude { get; set; }
        public decimal StartLongitude { get; set; }
        public decimal EndLatitude { get; set; }
        public decimal EndLongitude { get; set; }
        public string? CabType { get; set; }
        public int Passengers { get; set; }
        public DateTime BookingTime { get; set; }

        public decimal BaseFare { get; set; }
        public decimal Total { get; set; }
        public bool DiscountApplied { get; set; }


    }
}
