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

            dgrTrains.ItemsSource = infrabelService.TimeTableForSelectedStation;
        }

        private async void LstStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selection = lstStations.SelectedItem.ToString();
            await infrabelService.GetDeparturesAsync(selection);

            Task.Delay(1000);

            dgrTrains.ItemsSource = infrabelService.TimeTableForSelectedStation;

        }

        private void txtStationFilter_KeyUp(object sender, KeyEventArgs e)
        {
            string userInput = txtStationFilter.Text;

            FilteredStationsDisplay(userInput);
        }

        void PopulateStationsList()
        {
            lstStations.ItemsSource = infrabelService.StationsList.OrderBy(s => s.Name);
        }

        void FilteredStationsDisplay(string userInput)
        {

            lstStations.ItemsSource = infrabelService.StationsList
                .Where(s => s.Name.ToUpper().Contains(userInput.ToUpper()));
        }

       
    }
}