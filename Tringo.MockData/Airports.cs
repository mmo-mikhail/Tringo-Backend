namespace Tringo.MockData
{
    public class Airports
    {
        public Airports(string cityName, string iata, string lat, string lng)
        {
            CityName = cityName;
            Iata = iata;
            Lat = lat;
            Lng = lng;
        }

        public string CityName { get; set; }
        public string Iata { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
    }
}