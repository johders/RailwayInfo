using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Entities.Departures
{
    public class Departure
    {
        [JsonPropertyName("delay")]
        public string Delay { get; set; }

        [JsonPropertyName("station")]
        public string Station { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }


        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("canceled")]
        public string Canceled { get; set; }


        public DateTime ConvertedTime { get { return UnixTimeStampToDateTime(Time); } }



        public static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
        {
            double result = Convert.ToDouble(unixTimeStamp);
            
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(result).ToLocalTime();
            return dateTime;
        }
    }
}
