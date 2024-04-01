using Pre.Railway.Core.Entities;
using Pre.Railway.Core.Entities.Api.Departures;
using Pre.Railway.Core.Entities.Api.Station;
using Pre.Railway.Core.Event_Args;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
    public delegate void AnnounceDelayEventHandler(object sender, DelayEventArgs delayedTrain);
    public class InfrabelService
    {
        
        public event AnnounceDelayEventHandler AnnounceDelay;   



        const string stationsUrl = "https://api.irail.be/stations/?format=json&lang=nl";
        private readonly Random random = new Random();
        private int maxDelayInMinutes = 60;

        public List<TrainStation> StationsList { get; set; }
        public List<Departure> TimeTableForSelectedStation { get; set; }

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
            TimeTableForSelectedStation = new List<Departure>();

            using (HttpClient client = new HttpClient())
            {
                TimeTable departures = await client.GetFromJsonAsync<TimeTable>(departuresUrl);

                foreach (Departure departure in departures.Departures.Departure)
                {
                   TimeTableForSelectedStation.Add(departure);
                }
            }
        }

        public void PersonOnTracksDelay(List<Train> currentLiveBoard)
        {
            
            int count = currentLiveBoard.Count();
            long delayInMinutes = random.Next(1, maxDelayInMinutes + 1);
            
            int randomTrainIndex = random.Next(count);

            Train selectedTrain = currentLiveBoard.ElementAt(randomTrainIndex);

            selectedTrain.Delay = (delayInMinutes * 60).GetTime();

            AnnounceDelay?.Invoke(this, new DelayEventArgs(selectedTrain));

        }

        public void LeaveEarly(List<Train> currentLiveBoard)
        {
            currentLiveBoard.RemoveAt(0);
        }


        public async void DetectDelays(List<Train> currentLiveBoard)
        {
            foreach (Train train in currentLiveBoard)
            {
                while (!String.IsNullOrEmpty(train.Delay))
                {
                    AnnounceDelay?.Invoke(this, new DelayEventArgs(train));
                    await Task.Delay(10000);
                }
            }
        }


        // https://api.irail.be/stations/?format=json&lang=nl FOR STATION INFO
        // $"https://api.irail.be/liveboard/?station={station}&arrdep=departure&lang=nl&format=json&alerts=true FOR TIMETABLE INFO
    }
}
