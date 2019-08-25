using System;
using System.Collections.Generic;
using System.IO;

namespace Tringo.MockData
{
    public class MockData
    {
       /// <summary>
       /// Method to return a IEnumerable Airports List
       /// </summary>
       public static IEnumerable<Airports> GetAirPortsList()
       {
            using (var reader = new StreamReader("copy the path of airports.csv file to here"))
            {
                var airportslist = new List<Airports>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    airportslist.Add(new Airports(values[0], values[1], values[2], values[3]));
                }
                return airportslist;
            }
       }



       /// <summary>
       /// Method to return a IEnumerable Flights List
       /// </summary>
        public static IEnumerable<Flights> GetFlightsList()
        {
            var random = new Random();
            var AirPortsList = GetAirPortsList();
            var flightsList = new List<Flights>();
            for (var i = 0; i < 10; i++)
            {
                foreach (var Airports in AirPortsList)
                {
                    // get random price and Date
                    var price = random.Next(100, 1500);
                    var datetime = new DateTime(2019, random.Next(10, 11), random.Next(1, 29));
                    flightsList.Add(new Flights("SYD", Airports.Iata, price, datetime));
                }
            }
            return flightsList;
        }
    }
}
