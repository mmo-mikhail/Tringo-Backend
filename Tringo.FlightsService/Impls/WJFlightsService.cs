using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService.Impls
{
    public class WJFlightsService : IFlightsService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WJFlightsService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<ReturnFlightDestinationDto>> GetFlights(WJFlightsRequest WJFlightsRequest)
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
