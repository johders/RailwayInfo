using Pre.Railway.Core.Entities;
using Pre.Railway.Core.Entities.Departures;
using Pre.Railway.Core.Entities.Station;
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

        public List<TrainStation> StationsList { get; set; }
        public List<DepartureDisplay> TimeTableForSelectedStation { get; set; }

        public async Task GetStationsAsync()
        {
            StationsList = new List<TrainStation>();
            using (HttpClient client = new HttpClient())
            {
                StationList jsonResultList = await client.GetFromJsonAsync<StationList>(stationsUrl);

                foreach(TrainStation station in jsonResultList.Stations)
                {
                    StationsList.Add(station);
                }
            }
        }

        public async Task GetDeparturesAsync(string station)
        {
            string departuresUrl = $"https://api.irail.be/liveboard/?station={station}&arrdep=departure&lang=nl&format=json&alerts=true";
            TimeTableForSelectedStation = new List<DepartureDisplay>();

            using (HttpClient client = new HttpClient())
            {
                TimeTable departures = await client.GetFromJsonAsync<TimeTable>(departuresUrl);

                foreach (Departure departure in departures.Departures.Departure)
                {
                    DepartureDisplay newEntry = new DepartureDisplay(departure);
                    TimeTableForSelectedStation.Add(newEntry);
                }
            }
        }





        // https://api.irail.be/stations/?format=json&lang=nl FOR STATION INFO
        // $"https://api.irail.be/liveboard/?station={station}&arrdep=departure&lang=nl&format=json&alerts=true FOR TIMETABLE INFO
    }
}
