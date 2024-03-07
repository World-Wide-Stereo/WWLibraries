using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public class GoogleMaps
    {
        private const string OriginAddress = "123 Main Street, Town, CA 90210";

        public static string GetDirections(string streetAddress, string city, string state, string zipCode)
        {
            var directions = new StringBuilder();
            int stepCounter = 0;
            XDocument xmlDoc = XDocument.Load($"http://maps.googleapis.com/maps/api/directions/xml?origin={OriginAddress}&destination=" + streetAddress.URLEncode() + ", " + city.URLEncode() + ", " + state.URLEncode() + ", " + zipCode.URLEncode());


            string status = xmlDoc.Descendants("status").First().Value;
            if (status == "NOT_FOUND")
            {
                return "INVALID_DESTINATION";
            }
            
            string endAdressValue = "";
            XElement endAddress = xmlDoc.Descendants("end_address").LastOrDefault();
            if (endAddress != null)
                endAdressValue = endAddress.Value;
            
            if (!endAdressValue.Contains(city, true) || !endAdressValue.Contains(zipCode))
            {
                return "INVALID_DESTINATION";
            }

            XElement distance = xmlDoc.Descendants("distance").LastOrDefault();
            string distanceTotal = "";
            if (distance != null)
                distanceTotal = distance.Element("text").Value;
            
            XElement duration = xmlDoc.Descendants("duration").LastOrDefault();
            string durationTotal = "";
            if (duration != null)
                durationTotal = duration.Element("text").Value; 
             
            var steps = xmlDoc.Descendants("step").Select(x => new
            {
                duration = x.Element("duration").Element("text").Value,
                distance = x.Element("distance").Element("text").Value,
                instructions = x.Element("html_instructions").Value.StripHTMLTags(),
            });

            directions.Append("Total Distance: ").AppendLine(distanceTotal);
            directions.Append("Total Duration: ").Append(durationTotal).AppendLine(Environment.NewLine);
            foreach (var step in steps)
            {
                stepCounter++;

                if (stepCounter > 1)
                {
                    directions.AppendLine(Environment.NewLine);
                }
                directions.Append(stepCounter).Append(".").Append("\t").AppendLine(step.instructions);
                directions.Append("\t").Append(step.distance).Append("\t").Append(step.duration);
            }

            return directions.ToString();
        }
    }
}
