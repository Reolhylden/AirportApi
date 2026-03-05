namespace AirportApi.Models
{
    public class Flight
    {
        public string Flightnumber { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public string Gate { get; set; }
        public string Status { get; set; }
    }
}
