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
using Pre.Railway.Core.Event_Args;

namespace Pre.Railway.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Clock clock = new Clock(1000);
        InfrabelService infrabelService = new InfrabelService();
      
        public MainWindow()
        {

            Loaded += MainWindow_Loaded;

            infrabelService.CurrentStation = "Brugge";

            clock.ClockTick += Clock_ClockTick;
            clock.InfrabelService = infrabelService;
            clock.StartClockAsync();
            try
            {

                clock.SilentLiveBoardUpdateAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            lstStations.SelectionChanged += LstStations_SelectionChanged;
            infrabelService.ReportDelayToNmbs += InfrabelService_ReportDelayToNmbs;
            infrabelService.DetectDeparture += Infrabel_DetectDeparture;
            infrabelService.AutoUpdateLiveBoard += InfrabelService_AutoUpdateLiveBoard;

            await PopulateStationsAsync();
            await PopulateDeparturesAsync();

        }

        private void InfrabelService_AutoUpdateLiveBoard(object sender, EventArgs e)
        {
            List<Train> liveBoard = infrabelService.CurrentLiveBoard;
            dgrTrains.ItemsSource = liveBoard;

        }

        private async void LstStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            lblInfo.Content = "";

            if (lstStations.SelectedItem != null)
            {
                lblTitle.Content = "... loading ...";
                dgrTrains.ItemsSource = null;
                infrabelService.CurrentStation = lstStations.SelectedItem.ToString();

                await PopulateDeparturesAsync();
            }
        }

        private void Infrabel_DetectDeparture(object sender, ReportDepartureEventArgs e)
        {
            var departedTrain = e.DepartedTrain;
            var departedTrains = e.NmbsService.DepartedTrains;

            var alreadyAnnounced = departedTrains.Any(t => t.Equals(departedTrain));

            if (!alreadyAnnounced)
            {
                e.NmbsService.DepartedTrains.Add(e.DepartedTrain);
                UpdateLiveBoardAsync(e.NmbsService);
            }

        }

        private void InfrabelService_ReportDelayToNmbs(object sender, ReportDelayEventArgs e)
        {
            var delayedTrain = e.DelayedTrain;
            var delayedTrains = e.NmbsService.Delays;

            var alreadyAnnounced = delayedTrains.Any(t => t.Equals(delayedTrain));

            if (!alreadyAnnounced)
            {
                e.NmbsService.Delays.Add(e.DelayedTrain);
                UpdateLiveBoardAsync(e.NmbsService);
            }
        }

        private void Clock_ClockTick(object sender, EventArgs e)
        {
            if (lblTime != null)
            {
                lblTime.Content = clock.TimeString;
            }
        }

        private void TxtStationFilter_KeyUp(object sender, KeyEventArgs e)
        {
            string userInput = txtStationFilter.Text;

            FilteredStationsDisplay(userInput);
        }

        private void BtnPersonOnRails_Click(object sender, RoutedEventArgs e)
        {
            infrabelService.PersonOnTracksDelay();
            dgrTrains.ItemsSource = infrabelService.CurrentLiveBoard.ToList();
        }

        private void BtnAnnoyStudent_Click(object sender, RoutedEventArgs e)
        {
            infrabelService.LeaveEarly();
            dgrTrains.ItemsSource = infrabelService.CurrentLiveBoard.ToList();
        }

        private void BtnTestTextToSpeech_Click(object sender, RoutedEventArgs e)
        {
            string albertHam = "Look at me, I'm a train on a track I'm a train, I'm a train, I'm a chucka train, yeah";
            infrabelService.NmbsService.TestSpeechAsync(albertHam);
        }

        async Task PopulateStationsAsync()
        {
            try
            {
                await infrabelService.GetStationsAsync();
                PopulateStationsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async Task PopulateDeparturesAsync()
        {
            try
            {
                await infrabelService.GetDeparturesAsync();
                infrabelService.NmbsService.ChangeLogPath(infrabelService.CurrentStation);
                infrabelService.NmbsService.ClearPreviousStationInfo();
                UpdateTitle();

                List<Train> liveBoard = infrabelService.CurrentLiveBoard;
                dgrTrains.ItemsSource = liveBoard;

                //await ReadSpeechAnnouncementItemsAsync();

                infrabelService.ReportCurrentStationDelays();
                infrabelService.CompareCurrentWithDepartureTime(clock);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task UpdateLiveBoardAsync(NmbsService nmbsService)
        {
            
            nmbsService.UpdateLogFileAndLiveBoardAnnouncements();

            var announcements = nmbsService.LiveBoardAnnouncements;

            await Task.WhenAll(AnnounceEventsAsync(announcements), ReadQueuedAnnoucementsAsync());
        }

        async Task AnnounceEventsAsync(List<string> announcements)
        {
            foreach (string announcement in announcements)
            {
                lblInfo.Content = $"📢 Opgelet: {announcement}";
                await Task.Delay(10000);
            }
        }

        async Task ReadQueuedAnnoucementsAsync()
        {
            await infrabelService.NmbsService.ReadQueueAsync();
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

        void UpdateTitle()
        {
            lblTitle.Content = $"{infrabelService.CurrentStation}: Treinen bij vertrek";
        }

    }
}