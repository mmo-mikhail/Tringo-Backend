using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tringo.FlightsService;
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


        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightDestinationResponse>>> GetDestinationPrice()
        {
            // just for testing and will be deleted soon
            await Task.Delay(1000); // TODO: remove this line
            var response = new List<FlightDestinationResponse>()
            {
                new FlightDestinationResponse
                {
                    Lat = -31.9505,
                    Lng = 115.8605,
                    CityName = "Perth",
                    Price = 123,
                    PersonalPriorityIdx = 1,
                },
                new FlightDestinationResponse
                {
                    Lat = -12.4634,
                    Lng = 130.8456,
                    CityName = "Darwin",
                    Price = 400,
                    PersonalPriorityIdx = 1
                },
                new FlightDestinationResponse
                {
                    Lat = -33.8688,
                    Lng = 151.2093,
                    CityName = "Sydney",
                    Price = 99,
                    PersonalPriorityIdx = 1
                }
            };
            return new OkObjectResult(response);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<FlightDestinationResponse>>> GetDestinationPrices(
            [FromBody]FlightDestinationRequest inputData)
        {
            // just for testing and must be deleted soon
            await Task.Delay(100); // TODO: remove this line

            if (inputData?.Dates == null
                || inputData.SearchArea == null
                || inputData.Budget == null)
                return new BadRequestResult();

            // Find related flights
            var allFlights = _flightsService.GetFlights(inputData.DepartureAirportId);
            var allAirports = _flightsService.GetAirports();
            var relatedAirports = _destinationsFilter
                .FilterAirports(allAirports, inputData.SearchArea)
                .ToList();
            var airportsIatas = relatedAirports.Select(a => a.IataCode);

            // Base filtering:
            // - by Budget
            // - by flights to airports within requested area
            var fitleredFlights = allFlights
                .Where(f =>
                    inputData.Budget.Min < f.LowestPrice && f.LowestPrice < inputData.Budget.Max
                    && airportsIatas.Contains(f.To))
                .ToList();

            // Filter by Dates
            fitleredFlights = _destinationsFilter
                .FilterFlightsByDates(fitleredFlights, inputData.Dates)
                .ToList();

            // Map filtered flights to response
            var repsData = fitleredFlights.Select(f =>
            {
                var destinationAiport = relatedAirports.First(a => a.IataCode == f.To);
                return new FlightDestinationResponse
                {
                    Price = f.LowestPrice * inputData.NumberOfPeople,
                    CityName = destinationAiport.RelatedCityName,
                    Lat = destinationAiport.Lat,
                    Lng = destinationAiport.Lng,
                    PersonalPriorityIdx = 1
                };
            }).ToList();
            return new OkObjectResult(repsData);
        }
    }
}
