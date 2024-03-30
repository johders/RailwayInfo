using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Entities.Departures
{
    public class TimeTable
    {
        [JsonPropertyName("station")]
        public string Station { get; set; }

        [JsonPropertyName("departures")]
        public Departures Departures { get; set; }
    }
}
