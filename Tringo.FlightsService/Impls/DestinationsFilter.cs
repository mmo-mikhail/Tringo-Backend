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
				GetDistance(airport.Lat, airport.Lng, searchArea.Lat, searchArea.Lng) < searchArea.Radius);
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

		#region helpers
		private double GetDistance(double lat1, double lon1, double lat2, double lon2)
		{
			var R = 6371; // Radius of the earth in km
			var dLat = ToRadians(lat2 - lat1);  // deg2rad below
			var dLon = ToRadians(lon2 - lon1);
			var a =
				Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
			var d = R * c; // Distance in km
			return d;
		}

		private double ToRadians(double deg)
		{
			return deg * (Math.PI / 180);
		}
		#endregion
	}
}
