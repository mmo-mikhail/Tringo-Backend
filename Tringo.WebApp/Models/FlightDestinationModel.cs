
using System.ComponentModel.DataAnnotations;
using Tringo.FlightsService.DTO;

namespace Tringo.WebApp.Models
{

    public class FlightDestinationRequest
    {
        /// <summary>
        /// Area within which flights will be fitlered
        /// </summary>
        [Required(ErrorMessage = "Missing Search Area")]
        public SearchArea SearchArea { get; set; }

		/// <summary>
		/// Number of people requested. 1 by default
		/// </summary>
		[Range(1, 9)]
		public int NumberOfPeople { get; set; } = 1;

        /// <summary>
        /// Flight From airport id
        /// </summary>
        [Required(ErrorMessage = "Airport ID can't be null")]
        [StringLength(4, ErrorMessage = "maximum 4 characters")]
        public string DepartureAirportId { get; set; }

        /// <summary>
        /// Dates input. It can be specific or uncertain dates
        /// </summary>
        [Required(ErrorMessage = "Missing Dates")]
        public DatesRequest Dates { get; set; }

        public Budget Budget { get; set; }
	}

	public class Budget
	{
		public int Min { get; set; }
		public int Max { get; set; }
	}
}
