using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tringo.WebApp.Models;

namespace Tringo.WebApp.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly ILogger _logger;

        public FlightsController(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger<ValuesController>();
        }

        [Route("GetDestinationPrice")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightDestinationResponse>>> GetDestinationPrice()
        {
            // just for testing and will be deleted soon
            return await GetDestinationPrices(null);
        }

        [Route("GetDestinationPrices")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlightDestinationResponse>>> GetDestinationPrices(
            FlightDestinationRequest inputData = null)
        {
            await Task.Delay(1000); // TODO: remove this line
            var response = new List<FlightDestinationResponse>()
            {
                new FlightDestinationResponse
                {
                    Lat = 47.6062,
                    Lng = -122.3321,
                    CityName = "City Name X",
                    Price = 180
                },
                new FlightDestinationResponse
                {
                    Lat = 48.6062,
                    Lng = -122.9321,
                    CityName = "City Name Y",
                    Price = 150
                },
                new FlightDestinationResponse
                {
                    Lat = 45.2062,
                    Lng = -123.3321,
                    CityName = "City Name Z",
                    Price = 100
                }
            };
            if (inputData != null)
            {
                // Just another mock for now to verify request data are binded
                response.Add(new FlightDestinationResponse
                {
                    Lat = inputData.Lat,
                    Lng = inputData.Lng,
                    CityName = $"City {inputData.NumberOfPeople}",
                    Price = inputData.Dates != null
                        ? (inputData.Dates.DateUntil - inputData.Dates.DateFrom).TotalDays
                        : 99.99
                });
            }
            return new OkObjectResult(response);
        }
    }
}