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

        public bool OnlyPriceGuarantee { get; set; }

        public async Task<IEnumerable<ReturnFlightDestinationDto>> GetAllFlights(
            string from, int? departYear, int? departMonth, string travelType = "Economy")
        {
            var priceGuaranteeCodes = _airportsService.GetPriceGuaranteeAirportCodes();
            var newRequest = new WJFlightsRequest()
            {
                DepartureAirportCode = from,
                DepartYear = departYear,
                DepartMonth = departMonth,
                TravelClass = travelType,
                MaxPrice = null,
                DestinationAirportCodes = priceGuaranteeCodes
            };
            var priceGuaranteeDestinations = await PerformGetFlights(newRequest);
            return priceGuaranteeDestinations
                .Select(f => { f.From = from; return f; }) // API doesn't return from
                .ToList();
        }


            public async Task<IEnumerable<ReturnFlightDestinationDto>> GetFlights(
            WJFlightsRequest WJFlightsRequest)
        {
            var allFlights = await _memoryCacher.GetFromCacheAsync(
                $"{WJFlightsRequest.DepartureAirportCode}_{WJFlightsRequest.DepartYear}_{WJFlightsRequest.DepartMonth}" +
                $"_{WJFlightsRequest.TravelClass}_{OnlyPriceGuarantee}",
                async () =>
                {
                    var priceGuaranteeCodes = _airportsService.GetPriceGuaranteeAirportCodes();
                    var newRequest = new WJFlightsRequest()
                    {
                        DepartureAirportCode = WJFlightsRequest.DepartureAirportCode,
                        DepartYear = WJFlightsRequest.DepartYear,
                        DepartMonth = WJFlightsRequest.DepartMonth,
                        TravelClass = WJFlightsRequest.TravelClass,
                        MaxPrice = null,
                        DestinationAirportCodes = priceGuaranteeCodes
                    };
                    var priceGuaranteeDestinations = await PerformGetFlights(newRequest);
                    if (OnlyPriceGuarantee)
                    {
                        return priceGuaranteeDestinations
                            .Select(f => { f.From = WJFlightsRequest.DepartureAirportCode; return f; })
                            .ToList();
                    }
                    // Now request also other airports.
                    // But when sending all in one request - it doesn't work, so make 2 separate smaller ones
                    var otherIatas = _airportsService.GetOtherAirports()
                        .Select(a => a.IataCode).Where(code => !priceGuaranteeCodes.Contains(code));
                    newRequest.DestinationAirportCodes = otherIatas.Take(250);
                    var other1 = await PerformGetFlights(newRequest);
                    newRequest.DestinationAirportCodes = otherIatas.Skip(250);
                    var other2 = await PerformGetFlights(newRequest);
                    return priceGuaranteeDestinations
                        .Concat(other1).Concat(other2)
                        .Distinct(new A())
                        .Select(f => { f.From = WJFlightsRequest.DepartureAirportCode; return f; })
                        .ToList();
                }, TimeSpan.FromMinutes(5));

            // Filter only required airports
            allFlights = allFlights.Where(f =>
                WJFlightsRequest.DestinationAirportCodes.Contains(f.DestinationAirportCode)).ToList();

            var noPriceDestinations = WJFlightsRequest.DestinationAirportCodes
                .Except(allFlights.Select(f => f.DestinationAirportCode));
            
            // Filter by budget
            if (WJFlightsRequest.MaxPrice.HasValue)
            {
                allFlights = allFlights.Where(f => f.MinPrice < WJFlightsRequest.MaxPrice.Value).ToList();
            }
            
            // Add destinations without prices
            allFlights = allFlights.Concat(noPriceDestinations.Select(destCode => new ReturnFlightDestinationDto
            {
                From = WJFlightsRequest.DepartureAirportCode,
                DestinationAirportCode = destCode,
                MinPrice = -1
            })).ToList();

            // Exclude same from-to destinations just in case
            allFlights = allFlights.Where(f => f.From != f.DestinationAirportCode).ToList();

            return allFlights;
        }

        public Task<IEnumerable<ReturnFlightDestinationDto>> PerformGetFlights(WJFlightsRequest WJFlightsRequest)
        {
            return Task.Run(async () =>
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
            });
        }

        private class A : IEqualityComparer<ReturnFlightDestinationDto>
        {
            public bool Equals(ReturnFlightDestinationDto x, ReturnFlightDestinationDto y)
                => x.DestinationAirportCode == y.DestinationAirportCode;
            public int GetHashCode(ReturnFlightDestinationDto obj)
                => obj.DestinationAirportCode.GetHashCode();
        }
    }
}
