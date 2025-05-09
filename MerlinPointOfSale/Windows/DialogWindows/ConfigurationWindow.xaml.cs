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
using System.Windows.Shapes;

using MerlinPointOfSale.Windows.DialogWindows.DialogWindowsPages.ConfigurationWindowPages;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
        }

        private void OnBtnLocationSettings_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationFrame.Content = new LocationSettingsPage();
        }

        private void OnBtnTaxFianancialSettings_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationFrame.Content = new TaxFinancialSettingsPage();
        }

        private void OnBtnQuickSelectSettings_Click(object sender, RoutedEventArgs e)
        {
            QuickSelectSettingsPage quickSelectSettingsPage = new QuickSelectSettingsPage();
            quickSelectSettingsPage.QuickSelectUpdated += RefreshQuickSelect;
            ConfigurationFrame.Content = quickSelectSettingsPage;
        }

        private void RefreshQuickSelect()
        {
            if (Owner is PointOfSaleWindow posWindow)
            {
                posWindow.RefreshQuickSelect();
            }
        }

        private void OnBtnDatabaseSettings_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationFrame.Content = new DatabaseSettingsPage();
        }
    }
}
