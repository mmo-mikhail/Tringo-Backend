using System.Collections.Generic;
using System.Threading.Tasks;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService
{
    public interface IFlightsService
    {
        public Task<IEnumerable<ReturnFlightDestinationDto>> GetAllFlights(
            string from, int? departYear, int? departMonth, string travelType = "Economy");

        bool OnlyPriceGuarantee { get; set; }

        Task<IEnumerable<ReturnFlightDestinationDto>> GetFlights(WJFlightsRequest WJFlightsRequest);
    }
}
