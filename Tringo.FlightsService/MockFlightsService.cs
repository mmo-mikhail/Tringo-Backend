using Newtonsoft.Json;
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

        private static object flightsLock= new object();

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
                    Lat = double.Parse(lat),
                    Lng = double.Parse(lng)
                });
            }
            storedAirports = results;
            return storedAirports;
        }

        public static IEnumerable<FlightDestinationDto> GetFlights()
        {
            // usually the generated flights.json already exists on server,
            // but if not, it'll be re-generated

            const string flightsFileName = "flights.json";
            if (File.Exists(flightsFileName))
                return JsonConvert.DeserializeObject<IEnumerable<FlightDestinationDto>>(
                    File.ReadAllText(flightsFileName));

            lock (flightsLock)
            {
                if (File.Exists(flightsFileName))
                    return JsonConvert.DeserializeObject<IEnumerable<FlightDestinationDto>>(
                        File.ReadAllText(flightsFileName));

                using (File.Create(flightsFileName)) { }

                var airports = GetAirports().ToList();
                var flightsList = new List<FlightDestinationDto>();
                var random = new Random();
                var percentRandom = new Random();
                for (var i = 0; i < 40000; i++)
                {
                    // generate From airport:
                    // 50 % that it's Sydney and 30% for Melbourne. 20% for rest

                    var percent = percentRandom.NextDouble();
                    var from = "SYD";
                    if (percent > 0.5 && percent < 0.8)
                        from = "MEL";
                    else if (percent > 0.8)
                        from = airports[random.Next(0, airports.Count - 1)].IataCode;

                    // generate To airport
                    var to = airports[random.Next(0, airports.Count - 1)].IataCode;
                    while (to == from)
                        to = airports[random.Next(0, airports.Count - 1)].IataCode;

                    //generate unique date
                    if (!TryGetUniqueDate(flightsList, from, to, random, out DateTime date))
                        continue;

                    flightsList.Add(new FlightDestinationDto
                    {
                        From = from,
                        To = to,
                        Date = date,
                        LowestPrice = random.Next(100, 1500)
                    });
                }
                File.WriteAllText(flightsFileName,
                    JsonConvert.SerializeObject(flightsList));
                return flightsList;
            }
        }

        /// <summary>
        /// Tries to find the unusued date for particular flight. Returns false is not found
        /// </summary>
        private static bool TryGetUniqueDate(
            IEnumerable<FlightDestinationDto> flightsList,
            string from,
            string to,
            Random random,
            out DateTime date)
        {
            var counter = 0;
            var randomDate = new DateTime(2019, random.Next(10, 11), random.Next(1, 29));
            while (flightsList.Any(f => f.From == from && f.To == to
                && randomDate.Date == f.Date.Date))
            {
                if (counter++ > 30) // avoid infinite loop when all dates are blocked
                {
                    date = randomDate;
                    return false;
                }
                randomDate = new DateTime(2019, random.Next(10, 11), random.Next(1, 29));
            }
            date = randomDate;
            return true;
        }
    }
}
