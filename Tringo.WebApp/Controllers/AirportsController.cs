using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tringo.FlightsService;
using Tringo.FlightsService.DTO;
using Tringo.WebApp.Models;

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

            var airport = _airportsService.GetAirports().FirstOrDefault(a => a.IataCode == airportCode);
            return airport != null
                ? Ok(new Coordinates(airport.Lat, airport.Lng))
                : NoContent() as ActionResult;
        }
    }
}
