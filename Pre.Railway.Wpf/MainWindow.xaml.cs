﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Pre.Railway.Core.Services;
using Pre.Railway.Core.Entities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using Pre.Railway.Core.Entities.Api.Departures;
using static System.Collections.Specialized.BitVector32;
using System;

namespace Pre.Railway.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        InfrabelService infrabelService = new InfrabelService();
        Clock clock = new Clock(1000);

        public MainWindow()
        {
            Loaded += MainWindow_Loaded;
            clock.ClockTick += Clock_ClockTick;
            clock.StartClock();
        }     

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string initialStation = "Brugge";
            lstStations.SelectionChanged += LstStations_SelectionChanged;
            
            await PopulateStationsAsync();
            await PopulateDeparturesAsync(initialStation);
        }

        async Task PopulateStationsAsync()
        {
            await infrabelService.GetStationsAsync();
            PopulateStationsList();
        }

        async Task PopulateDeparturesAsync(string station)
        {

            await infrabelService.GetDeparturesAsync(station);
            UpdateTitle(station);

            var liveBoard = MapToLiveBoard(infrabelService.TimeTableForSelectedStation).ToList();
              
            dgrTrains.ItemsSource = liveBoard;

            infrabelService.ReportDelayToNmbs += InfrabelService_ReportDelayToNmbs;
            infrabelService.ReportDepartureToNmbs += InfrabelService_ReportDepartureToNmbs;
            infrabelService.DetectDeparture += Clock_DetectDeparture;

            infrabelService.ReportCurrentStationDelays(liveBoard);
            infrabelService.ReportTrainDeparture(liveBoard);
            infrabelService.CompareCurrentWithDepartureTime(clock, liveBoard, infrabelService.nmbsService);

        }

        private void Clock_DetectDeparture(object sender, Core.Event_Args.ReportDepartureEventArgs e)
        {
            e.NmbsService.DepartedTrains.Add(e.DepartedTrain);
            UpdateLiveBoard(e.NmbsService);
        }

        private void InfrabelService_ReportDepartureToNmbs(object sender, Core.Event_Args.ReportDepartureEventArgs e)
        {
            e.NmbsService.DepartedTrains.Add(e.DepartedTrain);
            UpdateLiveBoard(e.NmbsService);
        }

        private void InfrabelService_ReportDelayToNmbs(object sender, Core.Event_Args.ReportDelayEventArgs e)
        {

            e.NmbsService.Delays.Add(e.DelayedTrain);
            UpdateLiveBoard(e.NmbsService);                     
          
        }

        private void Clock_ClockTick(object sender, EventArgs e)
        {
            if(lblTime != null)
            {
                lblTime.Content = clock.TimeString;
            }
        }

        private async void LstStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            lblInfo.Content = string.Empty;

            if (lstStations.SelectedItem != null)
            {
                lblTitle.Content = "Loading...";
                string selection = lstStations.SelectedItem.ToString();
                await infrabelService.GetDeparturesAsync(selection);
                UpdateTitle(selection);
            }

            var updatedLiveBoard = MapToLiveBoard(infrabelService.TimeTableForSelectedStation).ToList();

            infrabelService.ReportCurrentStationDelays(updatedLiveBoard);
            infrabelService.ReportTrainDeparture(updatedLiveBoard);

            dgrTrains.ItemsSource = updatedLiveBoard;

        }

        private void TxtStationFilter_KeyUp(object sender, KeyEventArgs e)
        {
            string userInput = txtStationFilter.Text;

            FilteredStationsDisplay(userInput);
        }

        private void BtnPersonOnRails_Click(object sender, RoutedEventArgs e)
        {

            List<Train> currentLiveBoard = TakeLiveBoardScreenShot();
            infrabelService.PersonOnTracksDelay(currentLiveBoard);

            dgrTrains.ItemsSource = currentLiveBoard;

        }

        private void BtnAnnoyStudent_Click(object sender, RoutedEventArgs e)
        {
            List<Train> currentLiveBoard = TakeLiveBoardScreenShot();
            infrabelService.LeaveEarly(currentLiveBoard);

            dgrTrains.ItemsSource = currentLiveBoard;
        }

        async void UpdateLiveBoard(NmbsService nmbsService)
        {
            lblInfo.Content = string.Empty;
            nmbsService.UpdateLiveBoardAnnouncements();
            nmbsService.UpdateLogFileAnnouncements();

            var announcements = nmbsService.LiveBoardAnnouncements.ToList();

            foreach (string announcement in announcements)
            {

                lblInfo.Content = $"📢 Opgelet: {announcement}";
                await Task.Delay(10000);
            }
        }

        List<Train> TakeLiveBoardScreenShot()
        {
            return MapToLiveBoard(infrabelService.TimeTableForSelectedStation).ToList();
        }


        void PopulateStationsList()
        {
            lstStations.ItemsSource = infrabelService.StationsList
                .OrderBy(s => s.Name);
        }

        void FilteredStationsDisplay(string userInput)
        {

            var result = infrabelService.StationsList
               .Where(s => s.Name.ToUpper().StartsWith(userInput.ToUpper()));

            lstStations.ItemsSource = result;
        }

        IEnumerable<Train> MapToLiveBoard(List<Departure> departures)
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
                .ThenBy(t => t.Destination);
        }

        void UpdateTitle(string station)
        {
            lblTitle.Content = $"{station}: Treinen bij vertrek";
        }
        

    }
}