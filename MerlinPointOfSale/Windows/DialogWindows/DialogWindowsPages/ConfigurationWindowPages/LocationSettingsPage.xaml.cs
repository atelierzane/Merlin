using System.Windows;
using System.Windows.Controls;

namespace MerlinPointOfSale.Windows.DialogWindows.DialogWindowsPages.ConfigurationWindowPages
{
    public partial class LocationSettingsPage : Page
    {
        public LocationSettingsPage()
        {
            InitializeComponent();

            // Load settings into controls when the page is opened
            txtLocationID.Text = Properties.Settings.Default.LocationID;
            txtRegisterNumber.Text = Properties.Settings.Default.RegisterNumber;
            chkTradeHold.IsChecked = Properties.Settings.Default.LocationIsTradeHold;
            chkEnableTips.IsChecked = Properties.Settings.Default.TipsEnabled;
            chkEnableCommission.IsChecked = Properties.Settings.Default.CommissionEnabled;

            if (chkTradeHold.IsChecked == true)
            {
                stackTradeHoldDuration.Visibility = Visibility.Visible;
                txtTradeHoldDuration.Text = Properties.Settings.Default.LocationTradeHoldDuration.ToString();
            }

            // Hook event for trade hold checkbox
            chkTradeHold.Checked += (s, e) => stackTradeHoldDuration.Visibility = Visibility.Visible;
            chkTradeHold.Unchecked += (s, e) =>
            {
                stackTradeHoldDuration.Visibility = Visibility.Collapsed;
                txtTradeHoldDuration.Text = string.Empty; // Clear duration if unchecked
            };
        }

        private void onBtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Get the values from the text boxes
            string locationID = txtLocationID.Text.Trim();
            string registerNumber = txtRegisterNumber.Text.Trim();

            // Ensure the LocationID and RegisterNumber are not empty
            if (string.IsNullOrEmpty(locationID) || string.IsNullOrEmpty(registerNumber))
            {
                MessageBox.Show("Please enter both Location ID and Register Number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate trade hold duration if applicable
            int tradeHoldDuration = 0;
            if (chkTradeHold.IsChecked == true)
            {
                if (!int.TryParse(txtTradeHoldDuration.Text.Trim(), out tradeHoldDuration) || tradeHoldDuration <= 0)
                {
                    MessageBox.Show("Please enter a valid trade hold duration (in days).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Save the settings
            Properties.Settings.Default.LocationID = locationID;
            Properties.Settings.Default.RegisterNumber = registerNumber;
            Properties.Settings.Default.LocationIsTradeHold = chkTradeHold.IsChecked == true; // Save as a boolean
            Properties.Settings.Default.LocationTradeHoldDuration = tradeHoldDuration;
            Properties.Settings.Default.TipsEnabled = chkEnableTips.IsChecked == true; // Save tips setting
            Properties.Settings.Default.CommissionEnabled = chkEnableCommission.IsChecked == true; // Save commission setting
            Properties.Settings.Default.Save();

            MessageBox.Show("Location settings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
