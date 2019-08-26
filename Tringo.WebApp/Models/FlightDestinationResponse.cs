using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tringo.WebApp.Models
{
	public class FlightDestinationResponse
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
		/// Lowest Price found
		/// </summary>
		public double Price { get; set; }

		/// <summary>
		/// City or Airport title
		/// </summary>
		public string CityName { get; set; }
	}
}
