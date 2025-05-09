using MerlinAdministrator.Pages.OrganizationManagerPages;
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

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for OrganizationManagerLandingPage.xaml
    /// </summary>
    public partial class OrganizationManagerLandingPage : Page
    {
        public OrganizationManagerLandingPage()
        {
            InitializeComponent();
        }

        private void OnAddDistrict_Click(object sender, RoutedEventArgs e)
        {
            organizationManagerFrame.Content = new AddDistrictPage();
        }

        private void OnAddRegion_Click(object sender, RoutedEventArgs e)
        {
            organizationManagerFrame.Content = new AddRegionPage();
        }

        private void OnAddMarket_Click(object sender, RoutedEventArgs e)
        {
            organizationManagerFrame.Content = new AddMarketPage();
        }

        private void OnAddDivision_Click(object sender, RoutedEventArgs e)
        {
            organizationManagerFrame.Content = new AddDivisionPage();
        }

        private void OnEditDivision_Click(object sender, RoutedEventArgs e)
        {
            organizationManagerFrame.Content = new EditDivisionPage ();
        }

        private void OnEditMarket_Click(object sender, RoutedEventArgs e)
        {
            organizationManagerFrame.Content = new EditMarketPage();
        }

        private void OnEditRegion_Click(object sender, RoutedEventArgs e)
        {
            organizationManagerFrame.Content = new EditRegionPage();
        }

        private void OnEditDistrict_Click(object sender, RoutedEventArgs e)
        {
            organizationManagerFrame.Content = new EditDistrictPage();
        }
    }
}
