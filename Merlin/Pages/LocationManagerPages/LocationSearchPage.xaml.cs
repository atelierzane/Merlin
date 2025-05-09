using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.LocationManagerPages
{
    public partial class LocationSearchPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public LocationSearchPage()
        {
            InitializeComponent();
        }

        // Search button event handler
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Collect input values from the search fields
            string locationID = LocationIDTextBox.Text.Trim();
            string city = CityTextBox.Text.Trim();
            string phoneNumber = PhoneNumberTextBox.Text.Trim();
            string locationType = (LocationTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string managerID = ManagerIDTextBox.Text.Trim();

            LoadLocations(locationID, city, phoneNumber, locationType, managerID);
        }

        // Reset button event handler
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear search fields
            LocationIDTextBox.Clear();
            CityTextBox.Clear();
            PhoneNumberTextBox.Clear();
            LocationTypeComboBox.SelectedIndex = -1; // Reset selection
            ManagerIDTextBox.Clear();
            LocationDataGrid.ItemsSource = null; // Clear the data grid
        }

        // Method to load locations based on search criteria
        private void LoadLocations(string locationID, string city, string phoneNumber, string locationType, string managerID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Construct query
                    string query = "SELECT LocationID, LocationStreetAddress, LocationCity, LocationState, LocationZIP, LocationPhoneNumber, LocationManagerID, " +
                                   "LocationType, LocationIsTradeHold, LocationTradeHoldDuration " +
                                   "FROM Location WHERE 1=1"; // '1=1' allows us to append more conditions

                    // Append filtering conditions based on input
                    if (!string.IsNullOrEmpty(locationID))
                        query += " AND LocationID = @LocationID";
                    if (!string.IsNullOrEmpty(city))
                        query += " AND LocationCity LIKE @City";
                    if (!string.IsNullOrEmpty(phoneNumber))
                        query += " AND LocationPhoneNumber LIKE @PhoneNumber";
                    if (!string.IsNullOrEmpty(locationType))
                        query += " AND LocationType = @LocationType";
                    if (!string.IsNullOrEmpty(managerID))
                        query += " AND LocationManagerID = @ManagerID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters to the query
                        if (!string.IsNullOrEmpty(locationID))
                            cmd.Parameters.AddWithValue("@LocationID", locationID);
                        if (!string.IsNullOrEmpty(city))
                            cmd.Parameters.AddWithValue("@City", $"%{city}%");
                        if (!string.IsNullOrEmpty(phoneNumber))
                            cmd.Parameters.AddWithValue("@PhoneNumber", $"%{phoneNumber}%");
                        if (!string.IsNullOrEmpty(locationType))
                            cmd.Parameters.AddWithValue("@LocationType", locationType);
                        if (!string.IsNullOrEmpty(managerID))
                            cmd.Parameters.AddWithValue("@ManagerID", managerID);

                        List<Location> locations = new List<Location>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                locations.Add(new Location
                                {
                                    LocationID = reader["LocationID"].ToString(),
                                    LocationStreetAddress = reader["LocationStreetAddress"].ToString(),
                                    LocationCity = reader["LocationCity"].ToString(),
                                    LocationState = reader["LocationState"].ToString(),
                                    LocationZIP = reader["LocationZIP"].ToString(),
                                    LocationPhoneNumber = reader["LocationPhoneNumber"].ToString(),
                                    LocationManagerID = reader["LocationManagerID"].ToString(),
                                    LocationType = reader["LocationType"].ToString(),
                                    LocationIsTradeHold = (bool)reader["LocationIsTradeHold"],
                                    LocationTradeHoldDuration = reader["LocationTradeHoldDuration"] != DBNull.Value ? (int)reader["LocationTradeHoldDuration"] : 0
                                });
                            }
                        }

                        // Bind the results to the DataGrid
                        LocationDataGrid.ItemsSource = locations;
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
