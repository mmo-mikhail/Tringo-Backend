using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService.Impls
{
    public class AirportsService : IAirportsService
    {
        private static IEnumerable<AirportDto> _storedAirports;
        private static IEnumerable<AirportDto> _storedOtherAirports;

        private IList<string> _priceGuaranteeAirportCodes;

        public IList<string> GetPriceGuaranteeAirportCodes()
        {
            if (_priceGuaranteeAirportCodes == null)
                _priceGuaranteeAirportCodes = File.ReadAllLines("MockFiles/top200_airports.txt");
            return _priceGuaranteeAirportCodes;
        }

        public IEnumerable<AirportDto> GetPriceGuaranteeAirports()
        {
            return GetAirports(true);
        }

        public IEnumerable<AirportDto> GetOtherAirports()
        {
            return GetAirports(false);
        }

        public AirportDto GetAirport(string airportCode)
        {
            var results = new AirportDto();
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

                if (!iataCode.Equals(airportCode, StringComparison.InvariantCultureIgnoreCase)) continue;
                var coords = values[3].Split(',');
                var lng = coords[0].Replace("\"", "").Trim();
                var lat = coords[1].Replace("\"", "").Trim();
                results.IataCode = iataCode;
                results.Lat = double.Parse(lat);
                results.Lng = double.Parse(lng);
                break;
            }
            return results;
        }
        private IEnumerable<AirportDto> GetAirports(bool priceGuaranteeOnly)
        {
            if (priceGuaranteeOnly)
            {
                if (_storedAirports != null)
                    return _storedAirports;
            }
            else
            {
                if (_storedOtherAirports != null)
                    return _storedOtherAirports;
            }

            var results = new List<AirportDto>();
            var lines = File.ReadAllLines("MockFiles/airports.txt");
            var airportsPassangers = File.ReadAllText("MockFiles/AirportsPassengers.json");
            var airportsNamesWJJson = File.ReadAllText("MockFiles/AirportNamesWJ.json");
            var countriesCsv = File.ReadAllLines("MockFiles/countriesDB.csv")
                .Where(line => line.Contains(","))
                .Select(line => line.Split(','))
                .Where(a => a.Length == 2);

            var airports = JsonConvert.DeserializeObject<IEnumerable<AirportsData>>(airportsPassangers).ToList();
            var airportsNamesWJ = JsonConvert.DeserializeObject<AirportNamesWJModels>(airportsNamesWJJson).AirportCityInfo;

            var priceGuaranteeCodes = GetPriceGuaranteeAirportCodes();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split('\t');
                if (values.Length < 2)
                    continue;

                var type = values[0];
                if (priceGuaranteeOnly)
                {
                    if (type != "medium_airport" && type != "large_airport")
                        continue;
                }
                else
                {
                    if (type != "large_airport")
                        continue;
                }

                var iataCode = values[2];
                if (string.IsNullOrWhiteSpace(iataCode))
                    continue;

                var airportNameData = airportsNamesWJ.FirstOrDefault(n => n.TSAAirportCode == iataCode);
                if (airportNameData == null)
                {
                    continue;
                }
                if (priceGuaranteeOnly && !priceGuaranteeCodes.Contains(iataCode))
                {
                    continue;
                }

                var country = countriesCsv.FirstOrDefault(c => c[0] == airportNameData.CountryCode)[1];

                var coords = values[3].Split(',');
                var lng = coords[0].Replace("\"", "").Trim();
                var lat = coords[1].Replace("\"", "").Trim();

                var airportsData = airports.FirstOrDefault(a => a.IATACode == iataCode);
                results.Add(new AirportDto
                {
                    AirportName = airportNameData.AirportName,
                    RelatedCityName = airportNameData.CityName,
                    CountryName = country,
                    IataCode = iataCode,
                    Lat = double.Parse(lat),
                    Lng = double.Parse(lng),
                    NumberOfPassengers = airportsData == null ? default : airportsData.NumberofPassengers
                });
            }
            //results = results.OrderByDescending(a => a.NumberOfPassengers).Take(200).ToList();
            if (priceGuaranteeOnly)
                _storedAirports = results;
            else
                _storedOtherAirports = results;
            return results;
        }
    }
}
