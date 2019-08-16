
namespace Tringo.WebApp.Models
{

    public class FlightDestinationRequest
    {
        /// <summary>
        /// Latitude
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Lng { get; set; }

        /// <summary>
        /// Search Radius
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Number of people requested. 1 by default
        /// </summary>
        public int NumberOfPeople { get; set; } = 1;

        /// <summary>
        /// Flight From airport id. Default/unprovided is -1 
        /// </summary>
        public int DepartureAirportId { get; set; } = -1;

        /// <summary>
        /// Dates input. It can be specific or uncertain dates
        /// </summary>
        public DatesRequest Dates { get; set; }
    }

    public class FlightDestinationResponse
    {
        /// <summary>
        /// Latitude
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Lng { get; set; }

        /// <summary>
        /// Lowest Price found
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// City or Airport title
        /// </summary>
        public string CityName { get; set; }
    }
}
