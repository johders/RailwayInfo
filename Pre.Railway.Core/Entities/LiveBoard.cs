using Pre.Railway.Core.Entities.Departures;
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


        //public DepartureDisplay(Departure departure) 
        //{
        //    DepartureTime = departure.ConvertedTime.ToString("HH:mm");
        //    Delay = (int.Parse(departure.Delay) / 60).ToString();

        //    if(Delay == "0")
        //    {
        //        Delay = "";
        //    }

        //    Destination = departure.Station;
        //    Platform = departure.Platform;       
        //}
    }
}
