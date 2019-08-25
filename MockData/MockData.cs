using System;
using System.Collections.Generic;
using System.IO;

namespace MockData
{
    public class MockData
    {
        //Method to return a IEnumerable Airports List
        IEnumerable<Airports> GetAirPortsList()
        {
            using (var reader = new StreamReader("path of Airprots.csv"))
            {
                var airportslist = new List<Airports>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    airportslist.Add(new Airports(values[0], values[1], Convert.ToDouble(values[2]), Convert.ToDouble(values[3])));
                }

                IEnumerable<Airports> AirportsList = airportslist;
                return AirportsList;
            }
        }



        //Method to return a IEnumerable Flights List
        IEnumerable<Flights> GetFlightsList()
        {
            Random random = new Random();
            IEnumerable<Airports> AirPortsList = GetAirPortsList();
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

            IEnumerable<Flights> FlightsList = flightsList;
            return FlightsList;
        }

    }
}
