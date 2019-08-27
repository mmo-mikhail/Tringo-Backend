﻿using System;

namespace Tringo.FlightsService.DTO
{
    public class DatesRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateUntil { get; set; }

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
        public TravellingDurationTypes Duration { get; set; } = TravellingDurationTypes.Any;
    }

    [Flags]
    public enum TravellingDurationTypes
    {
        Any = 1,
        Weekend = 2,
        Week = 4,
        TwoWeeks = 8
    }
}