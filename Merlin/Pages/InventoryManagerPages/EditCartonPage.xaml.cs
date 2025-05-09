using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.InventoryManagerPages
{
    public partial class EditCartonPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming you have this helper

        public EditCartonPage()
        {
            InitializeComponent();
            LoadLocations(); // Load locations into ComboBoxes for Origin and Destination
        }

        // Load locations into ComboBoxes
        private void LoadLocations()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT LocationID FROM Location";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OriginComboBox.Items.Add(reader["LocationID"].ToString());
                                DestinationComboBox.Items.Add(reader["LocationID"].ToString());
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

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string cartonID = CartonIDTextBox.Text.Trim();
            if (string.IsNullOrEmpty(cartonID))
            {
                MessageBox.Show("Please enter a valid Carton ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT * FROM Cartons WHERE CartonID = @CartonID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CartonEditSection.Visibility = Visibility.Visible;

                                OriginComboBox.SelectedItem = reader["CartonOrigin"].ToString();
                                DestinationComboBox.SelectedItem = reader["CartonDestination"].ToString();
                                StatusComboBox.SelectedItem = reader["CartonStatus"].ToString();
                                ShipDatePicker.SelectedDate = Convert.ToDateTime(reader["CartonShipDate"]);
                                ReceiveDatePicker.SelectedDate = reader["CartonReceiveDate"] != DBNull.Value ? Convert.ToDateTime(reader["CartonReceiveDate"]) : (DateTime?)null;
                                ReceiveEmployeeTextBox.Text = reader["CartonReceiveEmployee"]?.ToString();
                            }
                            else
                            {
                                MessageBox.Show("Carton not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string cartonID = CartonIDTextBox.Text.Trim();
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"UPDATE Cartons 
                                     SET CartonOrigin = @Origin, CartonDestination = @Destination, 
                                         CartonStatus = @Status, CartonShipDate = @ShipDate, 
                                         CartonReceiveDate = @ReceiveDate, CartonReceiveEmployee = @ReceiveEmployee
                                     WHERE CartonID = @CartonID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Origin", OriginComboBox.SelectedItem);
                        cmd.Parameters.AddWithValue("@Destination", DestinationComboBox.SelectedItem);
                        cmd.Parameters.AddWithValue("@Status", StatusComboBox.SelectedItem);
                        cmd.Parameters.AddWithValue("@ShipDate", ShipDatePicker.SelectedDate ?? DateTime.Now);
                        cmd.Parameters.AddWithValue("@ReceiveDate", ReceiveDatePicker.SelectedDate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ReceiveEmployee", ReceiveEmployeeTextBox.Text);
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Carton updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
