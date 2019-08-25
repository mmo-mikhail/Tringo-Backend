using System;
using System.Collections.Generic;
using System.Text;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService
{
    public interface IFlightsService
    {
        IEnumerable<AirportDto> GetAirports();

        IEnumerable<FlightDestinationDto> GetFlights();
    }
}
