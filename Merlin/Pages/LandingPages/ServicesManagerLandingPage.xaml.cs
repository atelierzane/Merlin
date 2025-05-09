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
using MerlinAdministrator.Pages.ServicesManagerPages;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for ServicesManagerLandingPage.xaml
    /// </summary>
    public partial class ServicesManagerLandingPage : Page
    {
        public ServicesManagerLandingPage()
        {
            InitializeComponent();
        }

        private void OnAddService_Click(object sender, RoutedEventArgs e)
        {
            servicesManagerFrame.Content = new AddServicePage();
        }

        private void OnAddAddOns_Click(object sender, RoutedEventArgs e)
        {
            servicesManagerFrame.Content = new AddServiceAddOnsPage();
        }

        private void OnAddFees_Click(object sender, RoutedEventArgs e)
        {
            servicesManagerFrame.Content = new AddServiceFeePage();
        }

        private void OnServicesSearch_Click(object sender, RoutedEventArgs e)
        {
            servicesManagerFrame.Content = new ServiceSearchPage();
        }

        private void OnRemoveServices_Click(object sender, RoutedEventArgs e)
        {
            servicesManagerFrame.Content = new RemoveServiceBulkPage();
        }
    }
}
