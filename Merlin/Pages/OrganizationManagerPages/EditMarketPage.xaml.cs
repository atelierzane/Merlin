using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class EditMarketPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EditMarketPage()
        {
            InitializeComponent();
            LoadDivisions(); // Load divisions on page initialization
            LoadSupervisors();
        }

        // Load all divisions into the combo box for selection
        private void LoadDivisions()
        {
            try
            {
                DivisionComboBox.Items.Clear(); // Ensure the ComboBox is empty before loading

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT DivisionID, DivisionName FROM Divisions";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = $"{reader["DivisionName"]} (ID: {reader["DivisionID"]})",
                                    Tag = reader["DivisionID"].ToString()
                                };

                                DivisionComboBox.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading divisions: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load markets filtered by the selected division
        private void LoadMarkets(string divisionID)
        {
            try
            {
                MarketComboBox.Items.Clear(); // Clear previous markets

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT MarketID, MarketName FROM Markets WHERE DivisionID = @DivisionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionID", divisionID); // Bind the DivisionID parameter

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = $"{reader["MarketName"]} (ID: {reader["MarketID"]})",
                                    Tag = reader["MarketID"].ToString()
                                };

                                MarketComboBox.Items.Add(item);
                            }
                        }
                    }
                }

                // Check if no markets were loaded
                if (MarketComboBox.Items.Count == 0)
                {
                    MessageBox.Show("No markets found for the selected division.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading markets: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load supervisors into the combo box for editing
        private void LoadSupervisors()
        {
            try
            {
                MarketSupervisorComboBox.Items.Clear(); // Clear previous supervisors

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT EmployeeID, EmployeeFirstName + ' ' + EmployeeLastName AS Name FROM Employees WHERE EmployeeType = 'Supervisor'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = reader["Name"].ToString(),
                                    Tag = reader["EmployeeID"].ToString()
                                };

                                MarketSupervisorComboBox.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading supervisors: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handle division selection to load markets for the selected division
        private void DivisionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DivisionComboBox.SelectedItem is ComboBoxItem selectedDivision)
            {
                string divisionID = selectedDivision.Tag.ToString();

                // Load markets for the selected division
                LoadMarkets(divisionID);

                // Clear fields related to market details
                MarketNameTextBox.Clear();
                MarketSupervisorComboBox.SelectedIndex = -1;
            }
        }

        // Handle market selection and populate fields
        private void MarketComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarketComboBox.SelectedItem is ComboBoxItem selectedMarket)
            {
                string marketID = selectedMarket.Tag.ToString();
                LoadMarketDetails(marketID);
            }
        }

        private void LoadMarketDetails(string marketID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT MarketName, MarketSupervisorID FROM Markets WHERE MarketID = @MarketID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MarketID", marketID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                MarketNameTextBox.Text = reader["MarketName"].ToString();
                                string supervisorID = reader["MarketSupervisorID"].ToString();

                                foreach (ComboBoxItem item in MarketSupervisorComboBox.Items)
                                {
                                    if (item.Tag != null && item.Tag.ToString() == supervisorID)
                                    {
                                        MarketSupervisorComboBox.SelectedItem = item;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading market details: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Save changes to the market
        private void SaveMarket_Click(object sender, RoutedEventArgs e)
        {
            if (MarketComboBox.SelectedItem is not ComboBoxItem selectedMarket)
            {
                MessageBox.Show("Please select a market to edit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string marketID = selectedMarket.Tag.ToString();
            string marketName = MarketNameTextBox.Text.Trim();
            string supervisorID = (MarketSupervisorComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

            if (string.IsNullOrEmpty(marketName))
            {
                MessageBox.Show("Market name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "UPDATE Markets SET MarketName = @MarketName, MarketSupervisorID = @SupervisorID WHERE MarketID = @MarketID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MarketName", marketName);
                        cmd.Parameters.AddWithValue("@SupervisorID", supervisorID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MarketID", marketID);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Market updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error updating market: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Delete the selected market
        private void DeleteMarket_Click(object sender, RoutedEventArgs e)
        {
            if (MarketComboBox.SelectedItem is not ComboBoxItem selectedMarket)
            {
                MessageBox.Show("Please select a market to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string marketID = selectedMarket.Tag.ToString();

            var result = MessageBox.Show("Are you sure you want to delete this market? This action cannot be undone.", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        string query = "DELETE FROM Markets WHERE MarketID = @MarketID";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@MarketID", marketID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Market deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    MarketComboBox.Items.Remove(selectedMarket); // Remove from the ComboBox
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error deleting market: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
