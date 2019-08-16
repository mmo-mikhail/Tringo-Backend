
namespace Tringo.WebApp.Models
{

    public class FlightDestinationRequest
    {
        public double Lat { get; set; }
        public double Lng { get; set; }

        public double Radius { get; set; }

        public int NumberOfPeople { get; set; }

        public int DepartureAirportId { get; set; }

        public DatesRequest Dates { get; set; }
    }

    public class FlightDestinationResponse
    {
        public double Lat { get; set; }
        public double Lng { get; set; }

        public double Price { get; set; }

        public string CityName { get; set; }
    }
}
