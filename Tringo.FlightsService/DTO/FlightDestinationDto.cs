using System;

namespace Tringo.FlightsService.DTO
{
    public class FlightDestinationDto
    {
        public string From { get; set; }
        public string To { get; set; }
        public double LowestPrice { get; set; }
        public DateTime Date { get; set; }
    }
}
