using System;
using System.Collections.Generic;
using System.Text;

namespace Tringo.FlightsService.DTO
{
    public class AirportNamesWJModels
    {
        public List<AirportCityInfoModel> AirportCityInfo { get; set; }
    }

    public class AirportCityInfoModel
    {
        public string TSAAirportCode { get; set; }
        public string AirportName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
    }
}
