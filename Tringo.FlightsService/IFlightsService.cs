using System.Collections.Generic;
using System.Threading.Tasks;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService
{
    public interface IFlightsService
    {
        bool OnlyPriceGuarantee { get; set; }

        Task<IEnumerable<ReturnFlightDestinationDto>> GetFlights(WJFlightsRequest WJFlightsRequest);
    }
}
