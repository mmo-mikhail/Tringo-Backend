using System;

namespace Tringo.FlightsService.DTO
{
    public class FlightDestinationDto
    {
        public string From { get; set; }
        public string To { get; set; }
        public double LowestPrice { get; set; }
    }

	public class ReturnFlightDestinationDto : FlightDestinationDto
	{
		public DateTime DateDeparture { get; set; }
		public DateTime DateBack { get; set; }
	}
}
