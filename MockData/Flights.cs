using System;

namespace MockData
{
    public class Flights
    {
        public Flights(string from, string to, int price, DateTime date)
        {
            From = from;
            To = to;
            Price = price;
            Date = date;
        }

        public string From { get; set; }
        public string To { get; set; }
        public int Price { get; set; }
        public DateTime Date { get; set; }
    }
}