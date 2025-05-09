using System;
using System.Windows;
using System.Windows.Controls;

namespace MerlinPointOfSale.Pages.ReleaseConfigurationPages
{
    public partial class FinancialSettingsPage : Page
    {
        public FinancialSettingsPage()
        {
            InitializeComponent();
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtTaxRate.Text.Trim(), out decimal taxRate) && taxRate >= 0 && taxRate <= 100)
            {
                // Save logic here
                MessageBox.Show($"Tax Rate: {taxRate}%\nSaved Successfully!", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter a valid tax rate (0-100).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
