using Pre.Railway.Core.Entities.Api.Departures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pre.Railway.Core.Services
{
    public class ProgressService
    {
        public int PercentageComplete { get; set; } = 0;

        public List<Departure> DeparturesList { get; set; } = new List<Departure>();
    }
}
