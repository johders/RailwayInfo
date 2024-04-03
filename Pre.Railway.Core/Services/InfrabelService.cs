using Pre.Railway.Core.Entities;
using Pre.Railway.Core.Entities.Api.Departures;
using Pre.Railway.Core.Entities.Api.Station;
using Pre.Railway.Core.Event_Args;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public delegate void ReportDelayToNmbsEventHandler(object sender, ReportDelayEventArgs e);
    public delegate void ReportDepartureToNmbsEventHandler(object sender, ReportDepartureEventArgs e);

    public class InfrabelService
    {

        public event ReportDelayToNmbsEventHandler ReportDelayToNmbs;
        public event ReportDepartureToNmbsEventHandler ReportDepartureToNmbs;

        public event EventHandler<ReportDepartureEventArgs> DetectDeparture;

        const string stationsUrl = "https://api.irail.be/stations/?format=json&lang=nl";
        private readonly Random random = new Random();
        private int maxDelayInMinutes = 60;

        public List<TrainStation> StationsList { get; set; }
        public List<Departure> TimeTableForSelectedStation { get; set; }

        public NmbsService nmbsService { get; private set; } = new NmbsService();

        public async Task GetStationsAsync()
        {
            StationsList = new List<TrainStation>();
            using (HttpClient client = new HttpClient())
            {
                StationList jsonResultList = await client.GetFromJsonAsync<StationList>(stationsUrl);

                foreach (TrainStation station in jsonResultList.Stations)
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
                List<Departure> departuresOutput = departures.Departures.Departure;

                foreach (Departure departure in departuresOutput)
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


            //NEW
            //nmbsService.AffectedTrain = selectedTrain;
            //nmbsService.Delays.Add(selectedTrain);
            ReportDelayToNmbs?.Invoke(this, new ReportDelayEventArgs(nmbsService, selectedTrain));
            //nmbsService.LogAnnouncement(nmbsService.FormatTrainDelayEventInfo(selectedTrain));
            //nmbsService.WriteToLogFile();
        }

        public void LeaveEarly(List<Train> currentLiveBoard)
        {
            currentLiveBoard.RemoveAt(0);
        }

        public void ReportCurrentStationDelays(List<Train> currentLiveBoard)
        {
            nmbsService.Delays.Clear();
            foreach (Train train in currentLiveBoard)
            {
                if (!String.IsNullOrEmpty(train.Delay))
                {
                    //nmbsService.AffectedTrain = train;
                    //nmbsService.Delays.Add(train);
                    ReportDelayToNmbs?.Invoke(this, new ReportDelayEventArgs(nmbsService, train));
                    //nmbsService.LogAnnouncement(nmbsService.FormatTrainDelayEventInfo(train));

                    //Should only be logged fully on liveboard update??
                    //nmbsService.WriteToLogFile();
                }
            }
        }

        public void ReportTrainDeparture(List<Train> currentLiveBoard)
        {
            nmbsService.DepartedTrains.Clear();

            string currentTime = DateTime.Now.ToString("t");

            foreach(Train train in currentLiveBoard)
            {
                if(train.DepartureTime == currentTime)
                {
                    ReportDepartureToNmbs?.Invoke(this, new ReportDepartureEventArgs(nmbsService, train));
                    //nmbsService.DepartedTrains.Add(train);
                    //nmbsService.LogAnnouncement(nmbsService.FormatTrainDepartedEventInfo(train));
                }
            }
        }

        public void CompareCurrentWithDepartureTime(Clock clock, List<Train> currentLiveBoard, NmbsService nmbsService)
        {
            string timeString = String.Concat(clock.TimeString.Take(5));

            foreach (Train train in currentLiveBoard)
            {

                if (timeString == train.DepartureTime)
                {
                    DetectDeparture?.Invoke(this, new ReportDepartureEventArgs(nmbsService, train));
                }
            }

        }
    }
}
