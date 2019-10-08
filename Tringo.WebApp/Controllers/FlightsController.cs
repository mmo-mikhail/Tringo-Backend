using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly IFlightsService _flightsService;
        private readonly IAirportsService _airportsService;
        private readonly IDestinationsFilter _destinationsFilter;

        public FlightsController(ILoggerFactory logger,
            IConfiguration configuration,
            IFlightsService flightsService,
            IAirportsService airportsService,
            IDestinationsFilter destinationsFilter)
        {
            _logger = logger.CreateLogger(GetType());
            _configuration = configuration;
            _flightsService = flightsService;
            _airportsService = airportsService;
            _destinationsFilter = destinationsFilter;
        }

        /// <summary>
        /// Designed to operate with real WebJet API
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<IEnumerable<FlightDestinationResponse>>> GetAllLowestPrices(
            [FromBody]BaseFlightDestinationRequest inputData)
        {
            var departYear = inputData.Dates.MonthIdx != -1 
                ? inputData.Dates.MonthIdx < DateTime.Now.Month
                    ? DateTime.Now.Year + 1
                    : DateTime.Now.Year
                : (int?)null;
            var departMonth = inputData.Dates.MonthIdx != -1 ? inputData.Dates.MonthIdx + 1 : (int?)null;

            var allAirports = _airportsService.GetPriceGuaranteeAirports().ToList();
            var allFlights = (await _flightsService.GetAllFlights(inputData.DepartureAirportId, departYear, departMonth)).ToList();

            // Map filtered flights to response
            var repsData = MapToResponse(allFlights, allAirports, inputData.NumberOfPeople);
            return repsData.Count > 0 ? Ok(repsData) : NoContent() as ActionResult;
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
                reqData.DepartMonth = inputData.Dates.MonthIdx + 1;
                reqData.DepartYear = reqData.DepartMonth < DateTime.Now.Month
                        ? DateTime.Now.Year + 1
                        : DateTime.Now.Year;
            }

            if (!bool.TryParse(_configuration["OnlyPriceGuarantee"], out bool onlyPriceGuarantee))
            {
                onlyPriceGuarantee = true;
            }
            var allAirports = onlyPriceGuarantee
                ? _airportsService.GetPriceGuaranteeAirports()
                : _airportsService.GetPriceGuaranteeAirports().Concat(_airportsService.GetOtherAirports());
            var relatedAirports = _destinationsFilter
                .FilterAirports(allAirports, inputData.SearchArea)
                .ToList();
            reqData.DestinationAirportCodes = relatedAirports.Select(a => a.IataCode).Distinct();
            _flightsService.OnlyPriceGuarantee = onlyPriceGuarantee;
            var filteredFlights = (await _flightsService.GetFlights(reqData)).ToList();

            // Map filtered flights to response
            var repsData = MapToResponse(filteredFlights, relatedAirports, inputData.NumberOfPeople);
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
            var allAirports = _airportsService.GetPriceGuaranteeAirports();
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
            var repsData = MapToResponse(filteredFlights, relatedAirports, inputData.NumberOfPeople);
            return repsData.Count > 0 ? Ok(repsData) : NoContent() as ActionResult;
        }

        private IList<FlightDestinationResponse> MapToResponse(IList<ReturnFlightDestinationDto> fligths, IList<AirportDto> relatedAirports, int numberOfPeople)
        {
            return fligths.Select(f =>
            {
                var destinationAirport = relatedAirports.First(a => a.IataCode == f.DestinationAirportCode);
                var priorityIdx = FindPriorityIdx(relatedAirports, destinationAirport);
                return new FlightDestinationResponse
                {
                    Price = f.MinPrice * numberOfPeople,
                    DestAirportCode = destinationAirport.IataCode,
                    CityName = destinationAirport.RelatedCityName.Trim(),
                    AirportName = destinationAirport.AirportName?.Trim(),
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
        }


        private double FindPriorityIdx(IList<AirportDto> relatedAirports, AirportDto destinationAirport)
        {
            var popularAirports = _airportsService.GetPriceGuaranteeAirportCodes();
            var top200Idx = popularAirports.IndexOf(destinationAirport.IataCode);
            if (top200Idx != -1)
            {
                return FlightDestinationResponse.MaxPriorityIdx + (popularAirports.Count - top200Idx);
            }

            if (destinationAirport.NumberOfPassengers == default)
            {
                return -1;
            }
            var allValues = relatedAirports.Where(a => a.NumberOfPassengers != default).Select(a => a.NumberOfPassengers);
            var minValue = allValues.Min();
            // Find percentage between min-max number of passengers
            var localPercentage = 100.0 * (destinationAirport.NumberOfPassengers - minValue) / (allValues.Max() - minValue);

            var min = FlightDestinationResponse.LowestPriorityIdx;
            var max = FlightDestinationResponse.MaxPriorityIdx;

            // Find priority between max-min allowed values based on found percentage
            var priority = (localPercentage * (max - min) / 100.0) - min;
            return priority;
        }
    }
}
