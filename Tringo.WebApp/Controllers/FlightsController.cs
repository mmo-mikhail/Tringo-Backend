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
        private readonly IAirportsService _airportsService;
        private readonly IDestinationsFilter _destinationsFilter;

        public FlightsController(ILoggerFactory logger,
            IFlightsService flightsService,
            IAirportsService airportsService,
            IDestinationsFilter destinationsFilter)
        {
            _logger = logger.CreateLogger(GetType());
            _flightsService = flightsService;
            _airportsService = airportsService;
            _destinationsFilter = destinationsFilter;
        }

        /// <summary>
        /// Designed to operate with real WebJet API
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<IEnumerable<FlightDestinationResponse>>> GetLowestPrices(
            [FromBody]FlightDestinationRequest inputData)
        {
            var reqData = new WJFlightsRequest
            {
                DepartureAirportCode = inputData.DepartureAirportId,
                MaxPrice = inputData.Budget?.Max,
                TravelClass = "Economy"
            };
            if (inputData.Dates.MonthIdx != -1)
            {
                reqData.DepartYear = inputData.Dates.MonthIdx < DateTime.Now.Month
                        ? DateTime.Now.Year + 1
                        : DateTime.Now.Year;
                reqData.DepartMonth = inputData.Dates.MonthIdx + 1;
            }

            var allAirports = _airportsService.GetAirports();
            var relatedAirports = _destinationsFilter
                .FilterAirports(allAirports, inputData.SearchArea)
                .ToList();
            var airportsIatas = relatedAirports.Select(a => a.IataCode);
            reqData.DestinationAirportCodes = airportsIatas;

            var filteredFlights = (await _flightsService.GetFlights(reqData)).ToList();

            // Map filtered flights to response
            var repsData = filteredFlights.Select(f =>
            {
                var destinationAirport = relatedAirports.First(a => a.IataCode == f.DestinationAirportCode);
                var priorityIdx = FindPriorityIdx(relatedAirports, destinationAirport);
                return new FlightDestinationResponse
                {
                    Price = f.MinPrice * inputData.NumberOfPeople,
                    DestAirportCode = destinationAirport.IataCode,
                    CityName = destinationAirport.RelatedCityName,
                    Lat = destinationAirport.Lat,
                    Lng = destinationAirport.Lng,
                    PersonalPriorityIdx = priorityIdx,
                    FlightDates = new FlightDates
                    {
                        DepartureDate = f.DepartDate.Date,
                        ReturnDate = f.ReturnDate.Date
                    }
                };
            }).ToList();
            return repsData.Count > 0 ? Ok(repsData) : NoContent() as ActionResult;
        }

        /// <summary>
        /// Designed to operates with MOCK flights data only
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<IEnumerable<FlightDestinationResponse>>> GetDestinationPrices(
            [FromBody]FlightDestinationRequest inputData)
        {
            if (inputData == null)
                return new BadRequestResult();

            // Find related flights
            var allFlights = await _flightsService.GetFlights(
                new WJFlightsRequest { DepartureAirportCode = inputData.DepartureAirportId });
            var allAirports = _airportsService.GetAirports();
            var relatedAirports = _destinationsFilter
                .FilterAirports(allAirports, inputData.SearchArea)
                .ToList();
            var airportsIatas = relatedAirports.Select(a => a.IataCode);

            if (allFlights is null)
                return NoContent();

			// Filtering:
			// Filter by flights to airports within requested area
			var filteredFlights = allFlights.Where(f => airportsIatas.Contains(f.DestinationAirportCode)).ToList();

            // Filter by Budget
			if (inputData.Budget != null)
			{
				filteredFlights = filteredFlights.Where(f =>
					inputData.Budget.Min < f.MinPrice && f.MinPrice < inputData.Budget.Max).ToList();
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
                var destinationAirport = relatedAirports.First(a => a.IataCode == f.DestinationAirportCode);
                var priorityIdx = FindPriorityIdx(relatedAirports, destinationAirport);
                return new FlightDestinationResponse
                {
                    Price = f.MinPrice * inputData.NumberOfPeople,
                    DestAirportCode = destinationAirport.IataCode,
					CityName = destinationAirport.RelatedCityName,
                    Lat = destinationAirport.Lat,
                    Lng = destinationAirport.Lng,
					PersonalPriorityIdx = priorityIdx,
					FlightDates = new FlightDates
					{
                        DepartureDate = f.DepartDate.Date,
                        ReturnDate = f.ReturnDate.Date
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
