using System;

namespace Tringo.FlightsService.DTO
{
    public class DatesRequest
    {
        /// <summary>
        /// Month. Default/Any is -1 (minus one). Jan - 1, Feb - 2 and so on
        /// </summary>
        public int MonthIdx { get; set; } = -1;
    }
}
