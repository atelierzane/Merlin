using System.Windows;
using System.Windows.Controls;

namespace MerlinPointOfSale.Pages.ReleaseConfigurationPages
{
    public partial class LocationSettingsPage : Page
    {
        public LocationSettingsPage()
        {
            InitializeComponent();
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            string locationID = txtLocationID.Text.Trim();
            string registerNumber = txtRegisterNumber.Text.Trim();

            if (string.IsNullOrEmpty(locationID) || string.IsNullOrEmpty(registerNumber))
            {
                MessageBox.Show("Please fill out both fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Save logic here
            MessageBox.Show($"Location ID: {locationID}\nRegister: {registerNumber}\nSaved Successfully!", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
