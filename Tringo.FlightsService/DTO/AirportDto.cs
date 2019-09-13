
namespace Tringo.FlightsService.DTO
{
    public class AirportDto
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string IataCode { get; set; }
        public string AirportName { get; set; }
        public string RelatedCityName { get; set; }
        public int NumberOfPassengers { get; set; }
    }

    class AirportsData
    {
        public string IATACode { get; set; }

        public int NumberofPassengers { get; set; }
    }
}
