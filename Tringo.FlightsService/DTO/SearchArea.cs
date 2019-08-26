namespace Tringo.FlightsService.DTO
{
	public class SearchArea
	{
		/// <summary>
		/// Latitude
		/// </summary>
		public double Lat { get; set; }

		/// <summary>
		/// Longitude
		/// </summary>
		public double Lng { get; set; }

		/// <summary>
		/// Radius in Kilometers
		/// </summary>
		public int Radius { get; set; }
	}
}
