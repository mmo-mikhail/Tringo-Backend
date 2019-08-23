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
            FlightDestinationRequest inputData)
        {
            await Task.Delay(1000); // TODO: remove this line
            var response = new List<FlightDestinationResponse>()
            {
                
                new FlightDestinationResponse
                {
                    Iata = "PER",
                    Icao = "YPPH",
                    AirportName = "Perth International Airport",
                    CityName = "Perth",
                    Citycode = "PER",
                    Countrycode = "AU",
                    Utc = 8,
                    Lat =  -31.9403,
                    Lng = 115.9670029,
                    Active = 1,
                    Popularity = 85,
                    Price = 150
                },
                
                new FlightDestinationResponse
                {
                    Iata = "YPAD",
                    Icao = "ADL",
                    AirportName = "Adelaide International Airport",
                    CityName = "Perth",
                    Citycode = "ADL",
                    Countrycode = "AU",
                    Utc = 10.5,
                    Lat =  -34.9373,
                    Lng = 138.539,
                    Active = 1,
                    Popularity = 80,
                    Price = 140
                },
                
                new FlightDestinationResponse
                {
                    Iata = "BNE",
                    Icao = "YBBN",
                    AirportName = "Brisbane Airport",
                    CityName = "Brisbane",
                    Citycode = "BNE",
                    Countrycode = "AU",
                    Utc = 10,
                    Lat =  -27.470125,
                    Lng = 153.021072,
                    Active = 1,
                    Popularity = 85,
                    Price = 170
                },
                
                new FlightDestinationResponse
                {
                    Iata = "Darwin",
                    Icao = "YPDN",
                    AirportName = "Darwin International Airport",
                    CityName = "Darwin",
                    Citycode = "DRW",
                    Countrycode = "AU",
                    Utc = 10,
                    Lat =  -27.470125,
                    Lng = 153.021072,
                    Active = 1,
                    Popularity = 85,
                    Price = 170
                },
               
                new FlightDestinationResponse
                {
                    Iata = "MEL",
                    Icao = "YMML",
                    AirportName = "Melbourne Airport",
                    CityName = "Melbourne",
                    Citycode = "MEL",
                    Countrycode = "AU",
                    Utc = 10,
                    Lat =  -37.663712,
                    Lng = 144.844788,
                    Active = 1,
                    Popularity = 90,
                    Price = 200
                },
                new FlightDestinationResponse
                {
                    Iata = "CBR",
                    Icao = "YSCB",
                    AirportName = "Canberra Airport",
                    CityName = "Canberra",
                    Citycode = "CBR",
                    Countrycode = "AU",
                    Utc = 10,
                    Lat =  -35.305233,
                    Lng = 149.193393,
                    Active = 1,
                    Popularity = 80,
                    Price = 120
                },
                //I think we should hard code the departure airport to Sydney at this stage?
                //so we should not display airport pirce for Sydney right?
                new FlightDestinationResponse
                {
                    Lat = -33.8688,
                    Lng = 151.2093,
                    CityName = "Sydney",
                    Price = 99
                }
            };
            
            
            
            if (inputData != null)
            {
                // Just another mock for now to verify request data are binded
                response.Add(new FlightDestinationResponse
                {
                    Iata = inputData.Iata,
                    Icao = inputData.Icao,
                    AirportName = inputData.AirportName,
                    Citycode = inputData.Citycode,
                    Countrycode = inputData.Countrycode,
                    Utc = inputData.Utc,
                    Popularity = inputData.Popularity,
                    Active = inputData.Active,
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
