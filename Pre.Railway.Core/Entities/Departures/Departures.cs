using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Entities.Departures
{
    public class Departures
    {
        [JsonPropertyName("departure")]
        public List<Departure> Departure { get; set; }
    }
}
