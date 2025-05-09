using System;
using System.Windows;
using System.Windows.Controls;

namespace MerlinPointOfSale.Windows.DialogWindows.DialogWindowsPages.ConfigurationWindowPages
{
    public partial class TaxFinancialSettingsPage : Page
    {
        public TaxFinancialSettingsPage()
        {
            InitializeComponent();
            LoadTaxRate();
        }

        // Load the current tax rate from application settings
        private void LoadTaxRate()
        {
            decimal taxRate = Properties.Settings.Default.TaxRate;
            txtTaxRate.Text = taxRate.ToString("F2");  // Display tax rate with two decimal places
        }

        // Save the new tax rate when the save button is clicked
        private void onBtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtTaxRate.Text, out decimal taxRate))
            {
                Properties.Settings.Default.TaxRate = taxRate;
                Properties.Settings.Default.Save();
                MessageBox.Show("Tax rate saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter a valid tax rate.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
