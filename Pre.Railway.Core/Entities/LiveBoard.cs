using Pre.Railway.Core.Entities.Api.Departures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Entities
{
    public class LiveBoard
    {
        public string DepartureTime { get; set; }
        public string Delay { get; set; }
        public string Destination { get; set; }
        public string Platform { get; set; }
    }
}
