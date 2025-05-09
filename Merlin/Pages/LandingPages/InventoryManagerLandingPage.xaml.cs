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

using MerlinAdministrator.Pages.InventoryManagerPages;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for InventoryManagerLandingPage.xaml
    /// </summary>
    public partial class InventoryManagerLandingPage : Page
    {
        public InventoryManagerLandingPage()
        {
            InitializeComponent();
        }

        private void OnBtnInventorySearch_Click(object sender, RoutedEventArgs e)
        {
            inventoryManagerFrame.Content = new InventorySearchPage();
        }

        private void OnBtnAddCarton_Click(object sender, RoutedEventArgs e)
        {
            inventoryManagerFrame.Content = new CartonCreationPage();
        }

        private void OnBtnCartonSearch_Click(object sender, RoutedEventArgs e)
        {
            inventoryManagerFrame.Content = new CartonSearchPage();
        }

        private void OnBtnEditCarton_Click(object sender, RoutedEventArgs e)
        {
            inventoryManagerFrame.Content = new EditCartonPage();
        }

        private void OnBtnRemoveCarton_Click(object sender, RoutedEventArgs e)
        {
            inventoryManagerFrame.Content = new RemoveCartonPage();
        }

        private void OnBtnBulkRemoveCarton_Click(object sender, RoutedEventArgs e)
        {
            inventoryManagerFrame.Content = new RemoveCartonBulkPage();
        }
    }
}
