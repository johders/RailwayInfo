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
            FillInfo();
        }

        private void LstStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }


        void FillInfo()
        {
            lstStations.ItemsSource = infrabelService.StationsList;
        }
    }
}