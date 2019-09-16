using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tringo.FlightsService;
using Tringo.FlightsService.DTO;
using Tringo.WebApp.Models;

namespace Tringo.WebApp.Controllers
{
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IFlightsService _flightsService;
        private readonly IDestinationsFilter _destinationsFilter;

        public FlightsController(ILoggerFactory logger,
            IFlightsService flightsService,
            IDestinationsFilter destinationsFilter)
        {
            _logger = logger.CreateLogger(GetType());
            _flightsService = flightsService;
            _destinationsFilter = destinationsFilter;
        }


        [HttpPost]
        public async Task<ActionResult<IEnumerable<FlightDestinationResponse>>> GetDestinationPrices(
            [FromBody]FlightDestinationRequest inputData)
        {
            // just for testing and must be deleted soon
            await Task.Delay(100); // TODO: remove this line

            if (inputData == null)
                return new BadRequestResult();

            // Find related flights
            var allFlights = _flightsService.GetFlights(inputData.DepartureAirportId);
            var allAirports = _flightsService.GetAirports();
            var relatedAirports = _destinationsFilter
                .FilterAirports(allAirports, inputData.SearchArea)
                .ToList();
            var airportsIatas = relatedAirports.Select(a => a.IataCode);

            if (allFlights is null)
                return NoContent();

			// Filtering:
			// Filter by flights to airports within requested area
			var filteredFlights = allFlights.Where(f => airportsIatas.Contains(f.To)).ToList();

            // Filter by Budget
			if (inputData.Budget != null)
			{
				filteredFlights = filteredFlights.Where(f =>
					inputData.Budget.Min < f.LowestPrice && f.LowestPrice < inputData.Budget.Max).ToList();
			}

            // Filter by Dates
            filteredFlights = _destinationsFilter
                .FilterFlightsByDates(filteredFlights, inputData.Dates)
                .ToList();

			// It may happen that when unknown dates -> many same flights (with same from-to airports) filtered,
			// but with different prices
			// so need to select only MIN price for all same destinations
			filteredFlights = _destinationsFilter.FilterLowestPriceOnly(filteredFlights).ToList();

			// Map filtered flights to response
			var repsData = filteredFlights.Select(f =>
            {
                var destinationAirport = relatedAirports.First(a => a.IataCode == f.To);
                var priorityIdx = FindPriorityIdx(relatedAirports, destinationAirport);
                return new FlightDestinationResponse
                {
                    Price = f.LowestPrice * inputData.NumberOfPeople,
                    DestAirportCode = destinationAirport.IataCode,
					CityName = destinationAirport.RelatedCityName,
                    Lat = destinationAirport.Lat,
                    Lng = destinationAirport.Lng,
					PersonalPriorityIdx = priorityIdx,
					FlightDates = new FlightDates
					{
                        DepartureDate = f.DateDeparture.Date,
                        ReturnDate = f.DateBack.Date
                    }
                };
            }).ToList();
            return repsData.Count > 0 ? Ok(repsData) : NoContent() as ActionResult;
        }

        private int FindPriorityIdx(List<AirportDto> relatedAirports, AirportDto destinationAirport)
        {
            if (destinationAirport.NumberOfPassengers == default)
            {
                return -1;
            }
            var allValues = relatedAirports.Where(a => a.NumberOfPassengers != default).Select(a => a.NumberOfPassengers);
            var minValue = allValues.Min();
            var localPercentage = 100.0 * (destinationAirport.NumberOfPassengers - minValue) / allValues.Max() - minValue;

            var min = FlightDestinationResponse.LowestPriorityIdx;
            var max = FlightDestinationResponse.MaxPriorityIdx;

            var priority = (localPercentage * (max - min) / 100.0) - min;

            return (int)Math.Round(priority);
        }
    }
}
