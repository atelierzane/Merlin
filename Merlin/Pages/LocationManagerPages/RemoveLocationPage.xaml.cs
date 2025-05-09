using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.LocationManagerPages
{
    public partial class RemoveLocationPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public RemoveLocationPage()
        {
            InitializeComponent();
        }

        // Search for the location by LocationID
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
                    string query = "SELECT LocationStreetAddress, LocationCity, LocationState, LocationZIP, LocationPhoneNumber, LocationManagerID, LocationType " +
                                   "FROM Location WHERE LocationID = @LocationID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                // Populate the location information fields
                                LocationStreetAddressTextBlock.Text = reader["LocationStreetAddress"].ToString();
                                LocationCityTextBlock.Text = reader["LocationCity"].ToString();
                                LocationStateTextBlock.Text = reader["LocationState"].ToString();
                                LocationZIPTextBlock.Text = reader["LocationZIP"].ToString();
                                LocationPhoneNumberTextBlock.Text = reader["LocationPhoneNumber"].ToString();
                                LocationManagerIDTextBlock.Text = reader["LocationManagerID"].ToString();
                                LocationTypeTextBlock.Text = reader["LocationType"].ToString();

                                // Show the location information section
                                LocationInfoSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No location found with the given Location ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                LocationInfoSection.Visibility = Visibility.Collapsed;
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

        // Delete the location
        private void DeleteLocation_Click(object sender, RoutedEventArgs e)
        {
            string locationID = LocationIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(locationID))
            {
                MessageBox.Show("Please search for a location first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete Location ID: {locationID}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM Location WHERE LocationID = @LocationID";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@LocationID", locationID);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Location deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                // Clear the fields
                                LocationIDTextBox.Clear();
                                LocationStreetAddressTextBlock.Text = string.Empty;
                                LocationCityTextBlock.Text = string.Empty;
                                LocationStateTextBlock.Text = string.Empty;
                                LocationZIPTextBlock.Text = string.Empty;
                                LocationPhoneNumberTextBlock.Text = string.Empty;
                                LocationManagerIDTextBlock.Text = string.Empty;
                                LocationTypeTextBlock.Text = string.Empty;
                                LocationInfoSection.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete the location. Location ID not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
}
