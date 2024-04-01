using System.Text;
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
        NmbsService nmbsService;

        public MainWindow()
        {
            nmbsService = new NmbsService(infrabelService);
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

            List<Train> liveBoard = MapToLiveBoard(infrabelService.TimeTableForSelectedStation)
                .OrderBy(t => t.DepartureTime)
                .ThenBy(t => t.Destination).ToList();

            dgrTrains.ItemsSource = liveBoard;

            infrabelService.AnnounceDelay += InfrabelService_AnnounceDelay;
            nmbsService.UpdateLiveBoard(liveBoard);
        }

        private void Progress_ProgressChanged(object sender, ProgressService e)
        {
            pgbLoading.Value = e.PercentageComplete;
        }

        private void InfrabelService_AnnounceDelay(object sender, Core.Event_Args.DelayEventArgs delayedTrain)
        {
            lblInfo.Content = delayedTrain.LiveBoardMessage;
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
                string selection = lstStations.SelectedItem.ToString();
                await infrabelService.GetDeparturesAsync(selection);
                UpdateTitle(selection);
            }

            var updatedLiveBoard = MapToLiveBoard(infrabelService.TimeTableForSelectedStation);
            
            nmbsService.UpdateLiveBoard(updatedLiveBoard.ToList());

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