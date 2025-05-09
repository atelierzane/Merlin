using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MerlinAdministrator.Pages.LocationManagerPages;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for LocationManagerLandingPage.xaml
    /// </summary>
    public partial class LocationManagerLandingPage : Page
    {
        public LocationManagerLandingPage()
        {
            InitializeComponent();
        }

        private void BtnAddLocation_Click(object sender, RoutedEventArgs e)
        {
            locationManagerFrame.Content = new AddLocationPage();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            locationManagerFrame.Content = new LocationSearchPage();
        }

        private void BtnEditLocation_Click(object sender, RoutedEventArgs e)
        {
            locationManagerFrame.Content = new EditLocationPage();
        }

        private void BtnRemoveLocation_Click(object sender, RoutedEventArgs e)
        {
            locationManagerFrame.Content = new RemoveLocationPage();
        }
    }
}
