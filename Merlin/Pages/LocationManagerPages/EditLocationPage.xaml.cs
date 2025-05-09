using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.LocationManagerPages
{
    public partial class EditLocationPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EditLocationPage()
        {
            InitializeComponent();
        }

        // Search for the location by Location ID
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string locationID = LocationIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(locationID))
            {
                MessageBox.Show("Please enter a Location ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT * FROM Location WHERE LocationID = @LocationID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                // Populate the fields with location data
                                StreetAddressTextBox.Text = reader["LocationStreetAddress"].ToString();
                                CityTextBox.Text = reader["LocationCity"].ToString();
                                StateComboBox.SelectedValue = reader["LocationState"].ToString();
                                PhoneNumberTextBox.Text = reader["LocationPhoneNumber"].ToString();
                                ZIPTextBox.Text = reader["LocationZIP"].ToString();
                                ManagerComboBox.Text = reader["LocationManagerID"].ToString();
                                LocationTypeComboBox.SelectedValue = reader["LocationType"].ToString();
                                rbYes.IsChecked = (bool)reader["LocationIsTradeHold"];
                                TradeHoldDurationTextBox.Text = reader["LocationTradeHoldDuration"].ToString();

                                // Show the edit section
                                LocationEditSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No location found with the given Location ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                LocationEditSection.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Update the location information
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string locationID = LocationIDTextBox.Text.Trim();
            string streetAddress = StreetAddressTextBox.Text.Trim();
            string city = CityTextBox.Text.Trim();
            string state = StateComboBox.Text;
            string zip = ZIPTextBox.Text.Trim();
            string phoneNumber = PhoneNumberTextBox.Text.Trim();
            string managerID = ManagerComboBox.Text;
            string locationType = (LocationTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            bool isTradeHold = rbYes.IsChecked == true;
            int tradeHoldDuration = int.TryParse(TradeHoldDurationTextBox.Text, out int duration) ? duration : 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Location SET LocationStreetAddress = @StreetAddress, LocationCity = @City, " +
                                         "LocationState = @State, LocationZIP = @ZIP, LocationPhoneNumber = @PhoneNumber, " +
                                         "LocationManagerID = @ManagerID, LocationType = @Type, " +
                                         "LocationIsTradeHold = @IsTradeHold, LocationTradeHoldDuration = @TradeHoldDuration WHERE LocationID = @LocationID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@StreetAddress", streetAddress);
                        cmd.Parameters.AddWithValue("@City", city);
                        cmd.Parameters.AddWithValue("@State", state);
                        cmd.Parameters.AddWithValue("@ZIP", zip);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@ManagerID", managerID);
                        cmd.Parameters.AddWithValue("@Type", locationType);
                        cmd.Parameters.AddWithValue("@IsTradeHold", isTradeHold);
                        cmd.Parameters.AddWithValue("@TradeHoldDuration", tradeHoldDuration);
                        cmd.Parameters.AddWithValue("@LocationID", locationID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Location updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the location.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
