using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService.Impls
{
    public class MockFlightsService : IFlightsService
    {
        private static Dictionary<string, IEnumerable<ReturnFlightDestinationDto>> storedFlights
			= new Dictionary<string, IEnumerable<ReturnFlightDestinationDto>>();

		private static object flightsLock= new object();
        private readonly IAirportsService _airportsService;

        public MockFlightsService(IAirportsService airportsService)
        {
            _airportsService = airportsService;
        }

        public async Task<IEnumerable<ReturnFlightDestinationDto>> GetFlights(WJFlightsRequest WJFlightsRequest)
        {
            // usually the generated flights.json already exists on server,
            // but if not, it'll be re-generated

            var airportFromCode = WJFlightsRequest.DepartureAirportCode;
            var flightsFileName = $"MockFiles/flights-{airportFromCode}.json";

			if (storedFlights.ContainsKey(flightsFileName))
				return storedFlights[flightsFileName];

            await Task.Delay(10);
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

				var airports = _airportsService.GetAirports().ToList();
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
        /// Tries to find the unused date for particular flight.
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
            var randomDateDep = new DateTime(2019, random.Next(10, 11), random.Next(1, 30));
            var randomDateReturn = new DateTime(2019, random.Next(10, 11), random.Next(1, 30));
            while (randomDateDep.Date >= randomDateReturn.Date
				|| flightsList.Any(f => f.From == from && f.To == to
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
				randomDateDep = new DateTime(2019, random.Next(9, 11), random.Next(1, 30));
				randomDateReturn = new DateTime(2019, random.Next(9, 11), random.Next(1, 30));
            }
			departureDate = randomDateDep;
			returnDate = randomDateReturn;
			return true;
        }
    }
}
