using System;
using System.Collections.Generic;
using System.Linq;
using Tringo.FlightsService.DTO;

namespace Tringo.FlightsService.Impls
{
	public class DestinationsFilter : IDestinationsFilter
	{
		public IEnumerable<AirportDto> FilterAirports(
			IEnumerable<AirportDto> sourceAirports, SearchArea searchArea)
		{
            return sourceAirports.Where(airport =>
                WithinRectangle(
                    searchArea.Nw.Lat,
                    searchArea.Nw.Lng,
                    searchArea.Se.Lat,
                    searchArea.Se.Lng,
                    airport.Lat,
                    airport.Lng));
		}

		/// <summary>
		/// Filter out flights by date request
		/// for specific dates - [from,to) range used
		/// </summary>
		public IEnumerable<ReturnFlightDestinationDto> FilterFlightsByDates(
			IEnumerable<ReturnFlightDestinationDto> flights, DatesRequest dates)
		{
			if (dates.UncertainDates == null && (dates.DateFrom == null || dates.DateUntil == null))
				throw new ArgumentException($"Can't filter by dates: Inavlid argument {nameof(dates)}");

			if (dates.UncertainDates == null)
			{
				// Specific dates defined
				return flights.Where(f => dates.DateFrom.Value.Date == f.DateDeparture.Date
					&& f.DateBack.Date == dates.DateUntil.Value.Date);
			}
			else
			{
				if (dates.UncertainDates.MonthIdx != -1)
				{
                    var year = dates.UncertainDates.MonthIdx < DateTime.Now.Month
                        ? DateTime.Now.Year + 1
                        : DateTime.Now.Year;

                    flights = flights.Where(f => 
                        f.DateDeparture.Year == year && f.DateBack.Year == year
                        && f.DateDeparture.Month == dates.UncertainDates.MonthIdx
						&& f.DateBack.Month == dates.UncertainDates.MonthIdx);
				}
				//filter out by duration.
				switch (dates.UncertainDates.Duration)
				{
					case TravellingDurationTypes.Week:
						flights = flights.Where(f =>
							(f.DateBack.Date - f.DateDeparture.Date).TotalDays == 7);
						break;
					case TravellingDurationTypes.TwoWeeks:
						flights = flights.Where(f =>
							(f.DateBack.Date - f.DateDeparture.Date).TotalDays == 14);
						break;
					case TravellingDurationTypes.Weekend:
						flights = flights.Where(f =>
							(f.DateBack.Date - f.DateDeparture.Date).TotalDays == 1
							&& f.DateDeparture.DayOfWeek == DayOfWeek.Saturday);
						break;
					default:
						break;
				}

				return flights;
			}
		}

		/// <summary>
		/// Filters out only flights with min price if same flight (same from-to airports) present
		/// </summary>
		public IEnumerable<ReturnFlightDestinationDto> FilterLowestPriceOnly(
			IEnumerable<ReturnFlightDestinationDto> sourceAirports)
		{
			var cheapestFlights = new List<ReturnFlightDestinationDto>();
			foreach (var group in sourceAirports.GroupBy(flight =>new { flight.From, flight.To }))
			{
				var cheapestFlight = sourceAirports
					.Where(f => f.From == group.Key.From && f.To == group.Key.To)
					.OrderBy(f => f.LowestPrice).ElementAt(0);
				cheapestFlights.Add(cheapestFlight);
			}
			return cheapestFlights;
		}

		#region helpers

        // Just to show the idea with 180 lattitude;
        // First 4 parameters could be crammed into RectagleF
        // And last 2 parameters into PointF
        // https://stackoverflow.com/questions/23085122/check-whether-a-geographic-gps-point-on-a-map-lat-lon-is-inside-a-defined-rect
        public static bool WithinRectangle(
            double lattitudeNorth,
            double longitudeWest,
            double lattitudeSouth,
            double longitudeEast,
            double lattitude,
            double longitude)
        {
            if (lattitude > lattitudeNorth
                || lattitude < lattitudeSouth)
                return false;

            return longitudeEast >= longitudeWest
                ? longitude >= longitudeWest && longitude <= longitudeEast
                : longitude >= longitudeWest;
        }

        #endregion
    }
}
