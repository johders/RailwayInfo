using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Entities
{
    public class TrainStation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }


        public override string ToString()
        {
            return Name;
        }
        
    }
}
