using MerlinAdministrator.Pages.LoyaltyManagerPages;
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
    /// Interaction logic for LoyaltyManagerLandingPage.xaml
    /// </summary>
    public partial class LoyaltyManagerLandingPage : Page
    {
        public LoyaltyManagerLandingPage()
        {
            InitializeComponent();
        }

        private void BtnAddLoyalty_Click(object sender, RoutedEventArgs e)
        {
            loyaltyManagerFrame.Content = new AddLoyaltyPage();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            loyaltyManagerFrame.Content = new LoyaltySearchPage();
        }
    }
}
