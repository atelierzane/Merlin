using System;
using System.Data.SqlClient;
using System.Management;
using System.Net.NetworkInformation;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO.Ports;
using MerlinPointOfSale.Helpers;

namespace MerlinPointOfSale.Pages.ReleaseHomePages
{
    public partial class HomeDashboardPage : Page
    {
        private readonly DatabaseHelper _databaseHelper = new DatabaseHelper();
        private readonly ReceiptHelper receiptHelper = new ReceiptHelper();

        public HomeDashboardPage()
        {
            InitializeComponent();
            LoadStatusBarData();
        }

        private void LoadStatusBarData()
        {
            string connectionString = _databaseHelper.GetConnectionString();

            try
            {
                string locationID = Properties.Settings.Default.LocationID;

                if (string.IsNullOrEmpty(locationID))
                {
                    LocationInfoTextBlock.Text = "Not Set";
                    RegisterNumberTextBlock.Text = "Register: Not Set";
                    BusinessDayTextBlock.Text = "Business Day: Not Available";
                    UpdateStatusIndicator(false); // Default to closed
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Load location and business day info
                        LoadLocationAndBusinessDayData(connection, locationID);
                    }

                    RegisterNumberTextBlock.Text = $"Register: {Properties.Settings.Default.RegisterNumber}";
                }

                // Check hardware statuses
                CheckInternetConnectivity();
                CheckReceiptPrinterConnectivity();
                CheckBarcodeScannerConnectivity();
                CheckCashDrawerStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading status bar data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLocationAndBusinessDayData(SqlConnection connection, string locationID)
        {
            // Query for location information
            string locationQuery = @"
                SELECT LocationStreetAddress, LocationCity, LocationState
                FROM Location
                WHERE LocationID = @LocationID";

            using (SqlCommand cmd = new SqlCommand(locationQuery, connection))
            {
                cmd.Parameters.AddWithValue("@LocationID", locationID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string address = reader["LocationStreetAddress"].ToString().Trim();
                        string city = reader["LocationCity"].ToString().Trim();
                        string state = reader["LocationState"].ToString().Trim();
                        LocationInfoTextBlock.Text = $"{locationID} - {address}, {city}, {state}";
                    }
                    else
                    {
                        LocationInfoTextBlock.Text = $"{locationID} - Not Found";
                    }
                }
            }

            // Query for business day and status
            string businessDayQuery = @"
                SELECT BusinessDate, IsLocationOpen
                FROM LocationBusinessDay
                WHERE LocationID = @LocationID";

            using (SqlCommand cmd = new SqlCommand(businessDayQuery, connection))
            {
                cmd.Parameters.AddWithValue("@LocationID", locationID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DateTime businessDate = reader.GetDateTime(reader.GetOrdinal("BusinessDate"));
                        bool isLocationOpen = reader.GetBoolean(reader.GetOrdinal("IsLocationOpen"));

                        BusinessDayTextBlock.Text = $"Business Day: {businessDate:dddd, MMMM dd, yyyy}";
                        UpdateStatusIndicator(isLocationOpen);
                    }
                    else
                    {
                        BusinessDayTextBlock.Text = "Business Day: Not Found";
                        UpdateStatusIndicator(false);
                    }
                }
            }
        }

        private void UpdateStatusIndicator(bool isOpen)
        {
            StatusIndicator.Fill = isOpen ? Brushes.Green : Brushes.Red;
        }

        private void CheckInternetConnectivity()
        {
            bool isConnected = NetworkInterface.GetIsNetworkAvailable();
            InternetStatusIndicator.Fill = isConnected ? Brushes.Green : Brushes.Red;
        }

        private void CheckReceiptPrinterConnectivity()
        {
            bool isPrinterConnected = false;

            try
            {
                using (LocalPrintServer printServer = new LocalPrintServer())
                {
                    foreach (PrintQueue printer in printServer.GetPrintQueues())
                    {
                        if (printer.Name.Contains("EPSON TM-T88V Receipt"))
                        {
                            isPrinterConnected = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
                isPrinterConnected = false;
            }

            PrinterStatusIndicator.Fill = isPrinterConnected ? Brushes.Green : Brushes.Red;
        }

        private void CheckBarcodeScannerConnectivity()
        {
            bool isScannerConnected = false;

            try
            {
                // Query connected devices using WMI
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
                {
                    foreach (var device in searcher.Get())
                    {
                        var deviceName = device["Name"]?.ToString();
                        if (deviceName != null && deviceName.Contains("Symbol Bar Code Scanner::EA"))
                        {
                            isScannerConnected = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
                isScannerConnected = false; // Handle errors gracefully
            }

            ScannerStatusIndicator.Fill = isScannerConnected ? Brushes.Green : Brushes.Red;
        }

        private async void CheckCashDrawerStatus()
        {
            bool isCashDrawerOpen = false;
            string printerName = receiptHelper.GetPrinterName_Epson(); // Replace with your printer's name.

            await Task.Run(() =>
            {
                try
                {
                    // ESC/POS command to check the drawer status: DLE EOT 1
                    byte[] statusCommand = new byte[] { 0x10, 0x04, 0x01 };
                    byte[] response = RawPrinterHelper.SendBytesToPrinterWithResponse(printerName, statusCommand);

                    // Parse the response: bit 2 (0x04) indicates drawer status
                    if (response.Length > 0)
                    {
                        isCashDrawerOpen = (response[0] & 0x04) != 0;
                    }
                }
                catch (Exception ex)
                {
                    // Log or display the error message
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Error checking cash drawer status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });

            // Update the UI with the status
            Dispatcher.Invoke(() =>
            {
                CashDrawerStatusIndicator.Fill = isCashDrawerOpen ? Brushes.Green : Brushes.Red;
            });
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            if (scrollViewer == null)
                return;

            // Show the top shadow if the content is scrolled down
            TopShadow.Visibility = scrollViewer.VerticalOffset > 0 ? Visibility.Visible : Visibility.Collapsed;

            // Show the bottom shadow if the content is not fully scrolled
            double maxVerticalOffset = scrollViewer.ScrollableHeight;
            BottomShadow.Visibility = scrollViewer.VerticalOffset < maxVerticalOffset ? Visibility.Visible : Visibility.Collapsed;
        }


    }
}
