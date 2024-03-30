using Pre.Railway.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Net.WebRequestMethods;

namespace Pre.Railway.Core.Services
{
    public class InfrabelService
    {
        const string stationsUrl = "https://api.irail.be/stations/?format=json&lang=nl";

        public List<TrainStation> StationsList { get; set; } = new List<TrainStation>();

        public async Task GetStationsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                StationList jsonResultList = await client.GetFromJsonAsync<StationList>(stationsUrl);

                foreach(TrainStation station in jsonResultList.Stations)
                {
                    StationsList.Add(station);
                }
            }
        }







        // https://api.irail.be/stations/?format=json&lang=nl FOR STATION INFO
        // $"https://api.irail.be/liveboard/?station={station}&arrdep=departure&lang=nl&format=json&alerts=true FOR TIMETABLE INFO
    }
}
