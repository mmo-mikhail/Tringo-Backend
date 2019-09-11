using System;

namespace Tringo.FlightsService.DTO
{
    public class DatesRequest
    {
	    /// <summary>
	    /// Departure date.
	    /// WARN: it is null when unknown dates present! 
	    /// </summary>
		public DateTime? DateFrom { get; set; }

		/// <summary>
		/// Return date.
		/// WARN: it is null when unknown dates present! 
		/// </summary>
		public DateTime? DateUntil { get; set; }

		/// <summary>
		/// Represent unknown dates
		/// WARN: it is null when specific dates present! 
		/// </summary>
        public UncertainDatesRequest UncertainDates { get; set; }
    }

    public class UncertainDatesRequest
    {
        /// <summary>
        /// Month. Default/Any is -1 (minus one). Jan - 1, Feb - 2 and so on
        /// </summary>
        public int MonthIdx { get; set; } = -1;

        /// <summary>
        /// User asked to provide estimated duration of the travel
        /// </summary>
        public TravellingDurationTypes Duration { get; set; } = TravellingDurationTypes.None;
    }

    [Flags]
    public enum TravellingDurationTypes
    {
		/// <summary>
		/// None says that nothing is selected, which is considered as invalid state
		/// </summary>
	    None = 0,

		Weekend = 1,
        Week = 2,
        TwoWeeks = 4
    }
}
