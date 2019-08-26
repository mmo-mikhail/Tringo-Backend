
using Tringo.FlightsService.DTO;

namespace Tringo.WebApp.Models
{

    public class FlightDestinationRequest
    {
		public SearchArea SearchArea { get; set; }

		/// <summary>
		/// Number of people requested. 1 by default
		/// </summary>
		public int NumberOfPeople { get; set; } = 1;

        /// <summary>
        /// Flight From airport id
        /// </summary>
        public string DepartureAirportId { get; set; }

        /// <summary>
        /// Dates input. It can be specific or uncertain dates
        /// </summary>
        public DatesRequest Dates { get; set; }

		public Budget Budget { get; set; }
	}

	public class Budget
	{
		public int Min { get; set; }
		public int Max { get; set; }
	}
}
