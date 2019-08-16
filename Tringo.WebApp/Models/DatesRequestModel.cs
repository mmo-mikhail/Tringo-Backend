using System;

namespace Tringo.WebApp.Models
{
    public class DatesRequest
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateUntil { get; set; }

        public UncertainDatesRequest UncertainDates { get; set; }
    }

    public class UncertainDatesRequest
    {
        public int MonthIdx { get; set; }

        public TravellingDurationTypes Duration { get; set; }
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
