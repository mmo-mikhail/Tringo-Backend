using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace Tringo.AirportsParser
{
    class Program
    {
        private const string WIkiAirportsAddress = "https://en.wikipedia.org/wiki/List_of_international_airports_by_country";
        private const string outputFileName = "AirportsPassengers.json";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var airportsData = GatherAirportsData();
            var text = JsonConvert.SerializeObject(airportsData);
            if (!File.Exists(outputFileName))
            {
                using (File.Create(outputFileName)) { }
            }
            File.WriteAllText(outputFileName, text);
            Console.WriteLine("Done! Press any key to exit. Airports Found: " + airportsData.Count());
            Console.ReadKey();
        }

        static IEnumerable<AirportsData> GatherAirportsData()
        {
            var resultList = new List<AirportsData>();
            var htmlRaw = DownloadHtml();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlRaw);
            var tables = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'wikitable')]");
            foreach (HtmlNode tableNode in tables)
            {
                var body = tableNode.ChildNodes.FirstOrDefault(n => n.Name == "tbody");
                if (body == null)
                {
                    throw new Exception("Cannot find body");
                }
                var header = tableNode.ChildNodes.FirstOrDefault(n => n.Name == "theader");
                var passengersIdx = -1;
                if (header == null)
                {
                    header = body;
                }
                var passengersEl = header.ChildNodes.FirstOrDefault(n => n.Name == "tr").ChildNodes
                        .FirstOrDefault(el => el.InnerHtml.Contains(" Passengers"));
                if (passengersEl != null)
                {
                    passengersIdx = header.ChildNodes.FirstOrDefault(n => n.Name == "tr").ChildNodes
                        .Where(n => n.Name == "th").ToList().IndexOf(passengersEl);
                }

                if (passengersIdx == -1)
                    continue;

                // Now we know passengers column Idx + IATA column always present and is third

                var rows = body.ChildNodes.Where(n =>
                    n.Name == "tr" &&
                    !n.ChildNodes.Any(n2 => n2.Name == "th"));

                foreach (var row in rows)
                {
                    var cells = row.ChildNodes.Where(n => n.Name == "td").ToList();
                    var iataText = cells[2].InnerText.Replace("\n", "");
                    if (string.IsNullOrWhiteSpace(iataText))
                    {
                        continue;
                        //throw new Exception("IATA Code cannot be empty");
                    }
                    var passengersRaw = cells[passengersIdx].InnerText
                        .Replace("\n", "")
                        .Replace("~", "")
                        .Replace(">", "")
                        .Replace("&gt;", "");
                    var concatIdx = passengersRaw.IndexOf("&#");
                    if (concatIdx == -1) concatIdx = passengersRaw.IndexOf(" ");
                    if (concatIdx == -1) concatIdx = passengersRaw.IndexOf("[");
                    if (concatIdx != -1)
                    {
                        passengersRaw = passengersRaw.Substring(0, concatIdx);
                    }
                    if (!string.IsNullOrWhiteSpace(passengersRaw) && passengersRaw != "TBA")
                    {
                        if (int.TryParse(passengersRaw.Trim(), NumberStyles.AllowThousands,
                            CultureInfo.InvariantCulture, out int passengers))
                        {
                            var airportData = new AirportsData
                            {
                                IATACode = iataText,
                                NumberofPassengers = passengers
                            };
                            resultList.Add(airportData);
                        }
                        else
                        {
                            //throw new Exception("Parsing error");
                        }
                        
                    }
                }
            }
            return resultList;
        }

        private static string DownloadHtml()
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(WIkiAirportsAddress);
            }
        }
    }

    class AirportsData
    {
        public string IATACode { get; set; }

        public int NumberofPassengers { get; set; }
    }
}
