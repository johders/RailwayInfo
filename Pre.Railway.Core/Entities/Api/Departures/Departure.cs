using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Pre.Railway.Core.Services;

namespace Pre.Railway.Core.Entities.Api.Departures
{
    public class Departure
    {
        [JsonPropertyName("delay")]
        public long Delay { get; set; }

        [JsonPropertyName("station")]
        public string Station { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("canceled")]
        public string Canceled { get; set; }

        public string DepartureTimeConverted
        {
            get
            {
                long unixTime = Time;

                DateTime departureTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                departureTime = departureTime.AddSeconds(unixTime).ToLocalTime();

                DateTime midnight = new DateTime(departureTime.Year, departureTime.Month, departureTime.Day, 0, 0, 0);

                long difference = (long)(departureTime - midnight).TotalSeconds;
                
                return difference.GetTime();
            }
        }

        public string DelayTimeConverted { get { return Delay.GetTime(); } }

    }
}
