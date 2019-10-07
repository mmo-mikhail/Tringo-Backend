using System.Collections.Generic;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService
{
    public interface IAirportsService
    {
        IEnumerable<AirportDto> GetPriceGuaranteeAirports();
        IEnumerable<AirportDto> GetOtherAirports();
        AirportDto GetAirport(string airportCode);
        IList<string> GetPriceGuaranteeAirportCodes();
    }
}
