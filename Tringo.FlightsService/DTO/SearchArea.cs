namespace Tringo.FlightsService.DTO
{
	public class SearchArea
	{
        /// <summary>
        /// North West
        /// </summary>
        public Coordinates Nw { get; set; }

        /// <summary>
        /// South East
        /// </summary>
        public Coordinates Se { get; set; }
    }

    public class Coordinates
    {
        public Coordinates() { }

        public Coordinates(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }

        /// <summary>
		/// Latitude
		/// </summary>
		public double Lat { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Lng { get; set; }
    }
}
