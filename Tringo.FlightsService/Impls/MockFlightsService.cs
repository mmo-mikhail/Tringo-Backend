using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService.Impls
{
    public class MockFlightsService : IFlightsService
    {
        private static IEnumerable<AirportDto> storedAirports;
        private static Dictionary<string, IEnumerable<ReturnFlightDestinationDto>> storedFlights
			= new Dictionary<string, IEnumerable<ReturnFlightDestinationDto>>();

		private static object flightsLock= new object();


        public IEnumerable<AirportDto> GetAirports()
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

        public IEnumerable<ReturnFlightDestinationDto> GetFlights(string airportFromCode)
        {
            // usually the generated flights.json already exists on server,
            // but if not, it'll be re-generated

            var flightsFileName = $"MockFiles/flights-{airportFromCode}.json";

			if (storedFlights.ContainsKey(flightsFileName))
				return storedFlights[flightsFileName];

            if (File.Exists(flightsFileName))
			{
				var data = JsonConvert.DeserializeObject<IEnumerable<ReturnFlightDestinationDto>>(
					File.ReadAllText(flightsFileName));
				if (!storedFlights.ContainsKey(flightsFileName))
					storedFlights.Add(flightsFileName, data);
				return data;
			}
                

            lock (flightsLock)
            {
                using (File.Create(flightsFileName)) { }

				var airports = GetAirports().ToList();
                var flightsList = new List<ReturnFlightDestinationDto>();
                var random = new Random();
                var percentRandom = new Random();
				// be careful, 40k is really big number, it takes a lot to generate all the data
				for (var i = 0; i < 40000; i++)
                {
					// generate To airport
					var to = airports[random.Next(0, airports.Count - 1)].IataCode;
                    while (to == airportFromCode)
                        to = airports[random.Next(0, airports.Count - 1)].IataCode;

                    //generate unique date
                    if (!TryGetUniqueDate(flightsList, airportFromCode, to, random,
						out DateTime departureDate, out DateTime returnDate))
                        continue;

                    flightsList.Add(new ReturnFlightDestinationDto
					{
                        From = airportFromCode,
                        To = to,
						DateDeparture = departureDate.Date,
						DateBack = returnDate.Date,
                        LowestPrice = random.Next(100, 1500)
                    });
                }
				storedFlights.Add(flightsFileName, flightsList);

				File.WriteAllText(flightsFileName,
                    JsonConvert.SerializeObject(flightsList));

                return flightsList;
            }
        }

        /// <summary>
        /// Tries to find the unusued date for particular flight.
		/// Returns false is not found (all dates reserved)
        /// </summary>
        private static bool TryGetUniqueDate(
            IEnumerable<ReturnFlightDestinationDto> flightsList,
            string from,
            string to,
            Random random,
            out DateTime departureDate,
			out DateTime returnDate)
        {
            var counter = 0;
            var randomDateDep = new DateTime(2019, random.Next(10, 11), random.Next(1, 29));
            var randomDateReturn = new DateTime(2019, random.Next(10, 11), random.Next(1, 29));
            while (randomDateDep.Date > randomDateReturn.Date
				&& flightsList.Any(f => f.From == from && f.To == to
                && randomDateDep.Date == f.DateDeparture.Date 
				&& randomDateReturn.Date == f.DateBack.Date))
            {
                if (randomDateDep.Date < randomDateReturn.Date && counter++ > 100)
                {
					// avoid infinite loop when all dates are blocked
					departureDate = randomDateDep;
                    returnDate = randomDateReturn;
					return false;
                }
				randomDateDep = new DateTime(2019, random.Next(9, 11), random.Next(1, 29));
				randomDateReturn = new DateTime(2019, random.Next(9, 11), random.Next(1, 29));
            }
			departureDate = randomDateDep;
			returnDate = randomDateReturn;
			return true;
        }
    }
}
