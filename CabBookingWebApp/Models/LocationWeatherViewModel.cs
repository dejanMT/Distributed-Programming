namespace CabBookingWebApp.Models
{
    public class LocationWeatherViewModel
    {

        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string Result { get; set; } 
        public float Temperature { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
