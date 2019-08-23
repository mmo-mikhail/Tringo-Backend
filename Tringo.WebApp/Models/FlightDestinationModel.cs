
using System;

namespace Tringo.WebApp.Models
{

    public class FlightDestinationRequest
    {
        /// <summary>
        /// Latitude
        /// </summary>
        public double Lat { get; set; }
        
        /// <summary>
        /// Airport Iata code
        /// </summary>
        public string Iata {get; set;}
        /// <summary>
        /// Airport Icao code
        /// </summary>
        public string Icao {get; set;}
        /// <summary>
        /// airport name
        /// </summary>
        public string AirportName {get; set;}
        /// <summary>
        /// City code
        /// </summary>
        public string Citycode {get; set;}
        /// <summary>
        /// Country code
        /// </summary>
        public string Countrycode {get; set;}
        /// <summary>
        /// UTC time
        /// </summary>
        public double Utc {get; set;}
        /// <summary>
        /// Popularity of the airport
        /// </summary>
        public int Popularity {get; set;}
        /// <summary>
        /// Active = 1 represents the airport is in use
        /// </summary>
        public int Active {get; set;}
        
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
        /// <summary>
        /// Airport Iata code 
        /// </summary>
        public string Iata {get; set;}
        /// <summary>
        /// Airport Icao code
        /// </summary>
        
        public string Icao {get; set;}
        /// <summary>
        /// airport name
        /// </summary>
        public string AirportName {get; set;}
        /// <summary>
        /// City code
        /// </summary>
        public string Citycode {get; set;}
        /// <summary>
        /// Country code
        /// </summary>
        public string Countrycode {get; set;}
        /// <summary>
        /// UTC time
        /// </summary>
        public double Utc {get; set;}
        /// <summary>
        /// Popularity of the airport
        /// </summary>
        public int Popularity {get; set;}
        /// <summary>
        /// Active = 1 represents the airport is in use
        /// </summary>
        public int Active {get; set;}
    }
}
