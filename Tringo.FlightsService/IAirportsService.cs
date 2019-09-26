﻿using System.Collections.Generic;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService
{
    public interface IAirportsService
    {
        IEnumerable<AirportDto> GetAirports();

        IList<string> GetTop200Airports();
    }
}
