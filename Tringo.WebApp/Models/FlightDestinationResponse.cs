using System;
using Newtonsoft.Json;

namespace Tringo.WebApp.Models
{
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
        /// Total Lowest Price (including multiple passengers)
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// City or Airport title
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// Destination Airport IATA Code
        /// </summary>
        public string DestAirportCode { get; set; }

        /// <summary>
        /// Departure and Return Dates
        /// </summary>
        public FlightDates FlightDates { get; set; }

		/// <summary>
		/// Shows how importnat the flight destination for the user
		/// Higher number indicates higher priority
		/// </summary>
		public int PersonalPriorityIdx { get; set; }

        [JsonIgnore]
        public const int LowestPriorityIdx = 0;

        [JsonIgnore]
        public const int MaxPriorityIdx = 10;
    }

    public class FlightDates
    {
        public DateTime DepartureDate { get; set; }

        public DateTime ReturnDate { get; set; }
    }
}
