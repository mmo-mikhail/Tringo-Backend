using System;

namespace Tringo.FlightsService.DTO
{
    public class FlightDestinationDto
    {
        /// <summary>
        /// Used Only for MOCK data. To be deleted with removing Mock data
        /// </summary>
        public string From { get; set; }

        public string DestinationAirportCode { get; set; }
        public double MinPrice { get; set; }
    }

	public class ReturnFlightDestinationDto : FlightDestinationDto
	{
		public DateTime DepartDate { get; set; }
		public DateTime ReturnDate { get; set; }
	}
}
