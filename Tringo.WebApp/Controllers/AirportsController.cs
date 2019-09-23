using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Tringo.FlightsService;
using Tringo.FlightsService.DTO;

namespace Tringo.WebApp.Controllers
{
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    public class AirportsController : ControllerBase
    {
        private readonly IAirportsService _airportsService;

        public AirportsController(IAirportsService airportsService)
        {
            _airportsService = airportsService;
        }

        [HttpGet]
        public ActionResult<Coordinates> GetAirportCoordinates(string airportCode)
        {
            if (string.IsNullOrWhiteSpace(airportCode))
                return new BadRequestResult();

            var airport = _airportsService.GetAirports()
                .FirstOrDefault(a => a.IataCode == airportCode.ToUpperInvariant());
            return airport != null
                ? Ok(new Coordinates(airport.Lat, airport.Lng))
                : NoContent() as ActionResult;
        }
    }
}
