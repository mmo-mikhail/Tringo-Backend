using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService
{
    public class MockFlightsService
    {
        private static IEnumerable<AirportDto> storedAirports;

        public static IEnumerable<AirportDto> GetAirports()
        {
            if (storedAirports != null)
                return storedAirports;

            var results = new List<AirportDto>();
            var lines = File.ReadAllLines("MockFiles/airports.txt");
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split('\t');
                if (values.Length < 2)
                    continue;

                var type = values[0];
                if (type != "medium_airport" && type != "large_airport")
                    continue;

                var iataCode = values[2];
                if (string.IsNullOrWhiteSpace(iataCode))
                    continue;

                var coords = values[3].Split(',');
                var lat = coords[0].Replace("\"", "").Trim();
                var lng = coords[1].Replace("\"", "").Trim();
                results.Add(new AirportDto
                {
                    AirportName = values[1],
                    RelatedCityName = values[1]
                        .Replace("Airport", "")
                        .Replace("Air Base", "")
                        .Trim(),
                    IataCode = iataCode,
                    Lat = Double.Parse(lat),
                    Lng = Double.Parse(lng)
                });
            }
            storedAirports = results;
            return storedAirports;
        }

        public static IEnumerable<FlightDestinationDto> GetFlights()
        {
            var airports = GetAirports().ToList();
            var flightsList = new List<FlightDestinationDto>();
            for (var i = 0; i < 500; i++)
            {
                var random = new Random();

                foreach (var Airports in airports)
                {
                    // get random price and Date
                    var price = random.Next(100, 1500);
                    var datetime = new DateTime(2019, random.Next(10, 11), random.Next(1, 29));
                    const string from = "SYD";
                    var to = airports[random.Next(0, airports.Count - 1)].IataCode;
                    if (to == from)
                        to = airports[random.Next(0, airports.Count - 1)].IataCode;

                    flightsList.Add(new FlightDestinationDto
                    {
                        From = from,
                        To = to,
                        Date = datetime,
                        LowestPrice = price
                    });
                }
            }
            return flightsList;
        }
    }
}
