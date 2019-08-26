using System.Collections.Generic;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService
{
    public interface IFlightsService
    {
        IEnumerable<AirportDto> GetAirports();

        IEnumerable<ReturnFlightDestinationDto> GetFlights(string airportFromCode);
    }
}
