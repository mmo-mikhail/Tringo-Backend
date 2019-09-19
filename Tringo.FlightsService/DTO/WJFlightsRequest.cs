using System.Collections.Generic;

namespace Tringo.FlightsService.DTO
{
    public class WJFlightsRequest
    {
        /// <summary>
        /// IATA airport code – Mandatory
        /// </summary>
        public string DepartureAirportCode { get; set; }

        /// <summary>
        /// List of IATA airport codes – Mandatory
        /// </summary>
        public IEnumerable<string> DestinationAirportCodes { get; set; }

        /// <summary>
        /// Possible values Economy/PremiumEconomy/Business/First – Mandatory
        /// </summary>
        public string TravelClass { get; set; }

        /// <summary>
        /// Departure Year – Optional
        /// </summary>
        public int? DepartYear { get; set; }

        /// <summary>
        /// Departure Month – Optional ( only mandatory if Year is supplied)
        /// </summary>
        public int? DepartMonth { get; set; }

        /// <summary>
        /// Maximum price limit – Optional
        /// </summary>
        public int? MaxPriceLimit { get; set; }
    }
}
