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
using Pre.Railway.Core.Entities.Departures;
using System.Reflection;

namespace Pre.Railway.Wpf
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        InfrabelService infrabelService = new InfrabelService();

        public MainWindow()
        {
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lstStations.SelectionChanged += LstStations_SelectionChanged;

            await infrabelService.GetStationsAsync();
            PopulateStationsList();

            await infrabelService.GetDeparturesAsync("Brugge");

            Task.Delay(1000);

            dgrTrains.ItemsSource = MapToLiveBoard(infrabelService.TimeTableForSelectedStation)
                .OrderBy(t => t.DepartureTime)
                .ThenBy(t => t.Destination);
        }

        private async void LstStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstStations.SelectedItem != null)
            {
                string selection = lstStations.SelectedItem.ToString();
                await infrabelService.GetDeparturesAsync(selection);
            }

            Task.Delay(1000);

            dgrTrains.ItemsSource = MapToLiveBoard(infrabelService.TimeTableForSelectedStation)
                .OrderBy(t => t.DepartureTime)
                .ThenBy(t => t.Destination);

        }

        private void TxtStationFilter_KeyUp(object sender, KeyEventArgs e)
        {
            string userInput = txtStationFilter.Text;

            FilteredStationsDisplay(userInput);
        }

        private void BtnPersonOnRails_Click(object sender, RoutedEventArgs e)
        {

            List<DepartureDisplay> currentLiveBoard = TakeLiveBoardScreenShot();
            infrabelService.PersonOnTracksDelay(currentLiveBoard);

            dgrTrains.ItemsSource = currentLiveBoard;

        }

        private void BtnAnnoyStudent_Click(object sender, RoutedEventArgs e)
        {
            List<DepartureDisplay> currentLiveBoard = TakeLiveBoardScreenShot();
            infrabelService.LeaveEarly(currentLiveBoard);

            dgrTrains.ItemsSource = currentLiveBoard;
        }

        List<DepartureDisplay> TakeLiveBoardScreenShot()
        {
            return MapToLiveBoard(infrabelService.TimeTableForSelectedStation)
                .OrderBy(t => t.DepartureTime)
                .ThenBy(t => t.Destination).ToList();
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

        IEnumerable<DepartureDisplay> MapToLiveBoard(List<Departure> departures)
        {
            return departures
                .Select(d => new DepartureDisplay
                {
                    DepartureTime = d.ConvertedTime.ToString("HH:mm"),
                    Delay = ((int.Parse(d.Delay) / 60).ToString()) == "0" ? string.Empty : (int.Parse(d.Delay) / 60).ToString(),
                    Destination = d.Station,
                    Platform = d.Platform
                });
        }


    }
}