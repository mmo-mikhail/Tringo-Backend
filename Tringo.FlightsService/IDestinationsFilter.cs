using System.Collections.Generic;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService
{
	public interface IDestinationsFilter
	{
		IEnumerable<AirportDto> FilterAirports(
			IEnumerable<AirportDto> sourceAirports, SearchArea searchArea);

		IEnumerable<ReturnFlightDestinationDto> FilterFlightsByDates(
			IEnumerable<ReturnFlightDestinationDto> sourceAirports, DatesRequest dates);

		/// <summary>
		/// Filters out only flights with min price if same flight (same from-to airports) present
		/// </summary>
		IEnumerable<ReturnFlightDestinationDto> FilterLowestPriceOnly(
			IEnumerable<ReturnFlightDestinationDto> sourceAirports);
	}
}
