using Pre.Railway.Core.Entities.Api.Departures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Entities
{
    public class Train
    {
        public string DepartureTime { get; set; }
        public string Delay { get; set; }
        public string Destination { get; set; }
        public string Platform { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Train train &&
                   DepartureTime == train.DepartureTime &&
                   Destination == train.Destination &&
                   Platform == train.Platform;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DepartureTime, Destination, Platform);
        }
    }
}
