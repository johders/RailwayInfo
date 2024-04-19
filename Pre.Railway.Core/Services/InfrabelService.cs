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
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using static System.Net.WebRequestMethods;

namespace Pre.Railway.Core.Services
{
    public delegate void ReportDelayToNmbsEventHandler(object sender, ReportDelayEventArgs e);
    public delegate void AutoUpdateLiveBoardEvent(object sender, EventArgs e);

    public class InfrabelService
    {

        public event ReportDelayToNmbsEventHandler ReportDelayToNmbs;
        public event EventHandler<ReportDepartureEventArgs> DetectDeparture;
        public event AutoUpdateLiveBoardEvent AutoUpdateLiveBoard;

        const string stationsUrl = "https://api.irail.be/stations/?format=json&lang=nl";
        private readonly Random random = new Random();
        private int maxDelayInMinutes = 60;

        public List<TrainStation> StationsList { get; set; }
        public List<Departure> TimeTableForSelectedStation { get; set; }
        public string CurrentStation { get; set; }

        public List<Train> CurrentLiveBoard { get; private set; } = new List<Train>();

        public NmbsService NmbsService { get; private set; } = new NmbsService();

        public async Task GetStationsAsync()
        {
            StationsList = new List<TrainStation>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
					StationList jsonResultList = await client.GetFromJsonAsync<StationList>(stationsUrl);

					foreach (TrainStation station in jsonResultList.Stations)
					{
						StationsList.Add(station);
					}
				}
                catch (Exception ex)
                {
                    throw new Exception($"Er is een fout opgetreden: {ex.Message}");
                }

            }
        }

        public async Task GetDeparturesAsync(/*string station*/)
        {
            string departuresUrl = $"https://api.irail.be/liveboard/?station={CurrentStation}&arrdep=departure&lang=nl&format=json&alerts=true";
            TimeTableForSelectedStation = new List<Departure>();

            using (HttpClient client = new HttpClient())
            {
                try
                {
					TimeTable departures = await client.GetFromJsonAsync<TimeTable>(departuresUrl);
					List<Departure> departuresOutput = departures.Departures.Departure;

					foreach (Departure departure in departuresOutput)
					{
						TimeTableForSelectedStation.Add(departure);
					}
				}
                catch (Exception ex)
                {
					throw new Exception($"Er is een fout opgetreden: {ex.Message}");
				}
            }
            CurrentLiveBoard = MapToLiveBoard(TimeTableForSelectedStation);
        }

        List<Train> MapToLiveBoard(List<Departure> departures)
        {

            return departures
            .Select(d => new Train
            {
                DepartureTime = d.DepartureTimeConverted,
                Delay = d.DelayTimeConverted == "00:00" ? string.Empty : d.DelayTimeConverted,
                Destination = d.Station,
                Platform = d.Platform
            })
            .OrderBy(t => t.DepartureTime)
            .ThenBy(t => t.Destination).ToList();

        }

        public void LiveBoardUpdated()
        {
            AutoUpdateLiveBoard?.Invoke(this, EventArgs.Empty);
        }

        public void PersonOnTracksDelay()
        {

            int count = CurrentLiveBoard.Count();

            if(count == 0) return;

            long delayInMinutes = random.Next(1, maxDelayInMinutes + 1);

            int randomTrainIndex = random.Next(count);

            Train selectedTrain = CurrentLiveBoard.ElementAt(randomTrainIndex);

            selectedTrain.Delay = (delayInMinutes * 60).GetTime();

            ReportDelayToNmbs?.Invoke(this, new ReportDelayEventArgs(NmbsService, selectedTrain));
        }

        public void LeaveEarly()
        {

			int count = CurrentLiveBoard.Count();
			if (count == 0) return;

			Train train = CurrentLiveBoard.ElementAt(0);

			CurrentLiveBoard.RemoveAt(0);

            DetectDeparture?.Invoke(this, new ReportDepartureEventArgs(NmbsService, train));
        }

        public void ReportCurrentStationDelays(List<Train> currentLiveBoard)
        {
            NmbsService.Delays.Clear();
            foreach (Train train in currentLiveBoard)
            {
                if (!String.IsNullOrEmpty(train.Delay))
                {
                    ReportDelayToNmbs?.Invoke(this, new ReportDelayEventArgs(NmbsService, train));
                }
            }
        }

        public void CompareCurrentWithDepartureTime(Clock clock)
        {
            string timeString = String.Concat(clock.TimeString.Take(5));
            DateTime clockTime = DateTime.Parse(timeString);

            foreach (Train train in CurrentLiveBoard)
            {
                if (DateTime.Parse(train.DepartureTime) <= clockTime)
                {
                    DetectDeparture?.Invoke(this, new ReportDepartureEventArgs(NmbsService, train));
                }
            }

        }
    }
}
