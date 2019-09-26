using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tringo.FlightsService.DTO;
using System.Linq;

namespace Tringo.FlightsService.Impls
{
    public class WJFlightsService : IFlightsService
    {
        private readonly IAirportsService _airportsService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISimpleMemoryCacher _memoryCacher;

        public WJFlightsService(IAirportsService airportsService,
            IHttpClientFactory httpClientFactory,
            ISimpleMemoryCacher memoryCacher)
        {
            _airportsService = airportsService;
            _httpClientFactory = httpClientFactory;
            _memoryCacher = memoryCacher;
        }

        public async Task<IEnumerable<ReturnFlightDestinationDto>> GetFlights(
            WJFlightsRequest WJFlightsRequest)
        {
            var allFlights = await _memoryCacher.GetFromCacheAsync(
                $"{WJFlightsRequest.DepartureAirportCode}_{WJFlightsRequest.DepartYear}_{WJFlightsRequest.DepartMonth}_{WJFlightsRequest.TravelClass}",
                () =>
                {
                    var newRequest = new WJFlightsRequest()
                    {
                        DepartureAirportCode = WJFlightsRequest.DepartureAirportCode,
                        DepartYear = WJFlightsRequest.DepartYear,
                        DepartMonth = WJFlightsRequest.DepartMonth,
                        TravelClass = WJFlightsRequest.TravelClass,
                        MaxPrice = null,
                        DestinationAirportCodes = _airportsService.GetTop200Airports() //_airportsService.GetAirports().Where(a => top200.Contains(a.IataCode)).Select(a => a.IataCode)
                    };
                    return PerformGetFlights(newRequest);
                }, TimeSpan.FromMinutes(5));

            // Filter only required airports
            allFlights = allFlights.Where(f => WJFlightsRequest.DestinationAirportCodes.Contains(f.DestinationAirportCode));

            var noPriceDestinations = WJFlightsRequest.DestinationAirportCodes.Except(allFlights.Select(f => f.DestinationAirportCode));
            
            // Filter by budget
            if (WJFlightsRequest.MaxPrice.HasValue)
            {
                allFlights = allFlights.Where(f => f.MinPrice < WJFlightsRequest.MaxPrice.Value);
            }
            
            // Add destinations without prices
            allFlights = allFlights.Union(noPriceDestinations.Select(destCode => new ReturnFlightDestinationDto
            {
                From = WJFlightsRequest.DepartureAirportCode,
                DestinationAirportCode = destCode,
                MinPrice = -1
            }));

            allFlights = allFlights.Where(f => f.From != f.DestinationAirportCode);

            return allFlights;
        }

        public async Task<IEnumerable<ReturnFlightDestinationDto>> PerformGetFlights(WJFlightsRequest WJFlightsRequest)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                "api/flights/dealfinder/insights/cheapestbydeparture");

            var json = JsonConvert.SerializeObject(WJFlightsRequest, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Method = new HttpMethod("POST");
            var client = _httpClientFactory.CreateClient("webjet");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<ReturnFlightDestinationDto>>(res);
            }
            else
            {
                return Array.Empty<ReturnFlightDestinationDto>();
            }
        }
    }
}
