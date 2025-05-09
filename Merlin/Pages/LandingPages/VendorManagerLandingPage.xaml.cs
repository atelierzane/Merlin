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

using MerlinAdministrator.Pages.VendorManagerPages;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for VendorManagerLandingPage.xaml
    /// </summary>
    public partial class VendorManagerLandingPage : Page
    {
        public VendorManagerLandingPage()
        {
            InitializeComponent();
        }

        private void OnBtnAddVendor_Click(object sender, RoutedEventArgs e)
        {
            vendorManagerFrame.Content = new AddVendorPage();
        }

        private void OnBtnEditVendor_Click(object sender, RoutedEventArgs e)
        {
            vendorManagerFrame.Content = new EditVendorPage();
        }

        private void OnBtnVendorSearch_Click(object sender, RoutedEventArgs e)
        {
            vendorManagerFrame.Content = new VendorSearchPage();
        }

        private void OnBtnRemoveVendor_Click(object sender, RoutedEventArgs e)
        {
            vendorManagerFrame.Content = new RemoveVendorPage();
        }
    }
}
