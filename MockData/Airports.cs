namespace MockData
{
    public class Airports
    {
        public Airports(string cityName, string iata, double lat, double lng)
        {
            CityName = cityName;
            Iata = iata;
            Lat = lat;
            Lng = lng;
        }

        public string CityName { get; set; }
        public string Iata { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}