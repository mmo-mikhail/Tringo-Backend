using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService.Impls
{
    public class AirportsService : IAirportsService
    {
        private static IEnumerable<AirportDto> storedAirports;

        public IEnumerable<AirportDto> GetAirports()
        {
            if (storedAirports != null)
                return storedAirports;

            var results = new List<AirportDto>();
            var lines = File.ReadAllLines("MockFiles/airports.txt");
            var airportsPassangers = File.ReadAllText("MockFiles/AirportsPassengers.json");
            var airportsNamesWJ = File.ReadAllText("MockFiles/AirportNamesWJ.json");
            var airports = JsonConvert.DeserializeObject<IEnumerable<AirportsData>>(airportsPassangers).ToList();
            var airportsNames = JsonConvert.DeserializeObject<AirportNamesWJModels>(airportsNamesWJ).AirportCityInfo;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split('\t');
                if (values.Length < 2)
                    continue;

                var type = values[0];
                if (type != "large_airport")
                    //if (type != "medium_airport" && type != "large_airport") // Exclude Medium airports for MVP
                    continue;

                var iataCode = values[2];
                if (string.IsNullOrWhiteSpace(iataCode))
                    continue;

                var airportNameData = airportsNames.FirstOrDefault(n => n.TSAAirportCode == iataCode);
                if (airportNameData == null)
                {
                    continue;
                }

                var coords = values[3].Split(',');
                var lng = coords[0].Replace("\"", "").Trim();
                var lat = coords[1].Replace("\"", "").Trim();

                var airportsData = airports.FirstOrDefault(a => a.IATACode == iataCode);
                results.Add(new AirportDto
                {
                    AirportName = values[1],
                    RelatedCityName = airportNameData.CityName,
                    IataCode = iataCode,
                    Lat = double.Parse(lat),
                    Lng = double.Parse(lng),
                    NumberOfPassengers = airportsData == null ? default : airportsData.NumberofPassengers
                });
            }
            var ads = results.Where(aa => aa.NumberOfPassengers < 1).ToList();
            //results = results.OrderByDescending(a => a.NumberOfPassengers).Take(200).ToList();
            //storedAirports = results;
            return results;
        }
    }
}
