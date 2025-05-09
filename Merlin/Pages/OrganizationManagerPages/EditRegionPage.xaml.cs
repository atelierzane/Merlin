using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class EditRegionPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EditRegionPage()
        {
            InitializeComponent();
            LoadDivisions(); // Load divisions at initialization
            LoadSupervisors(); // Load supervisors at initialization
        }

        // Load all divisions into the DivisionComboBox
        private void LoadDivisions()
        {
            try
            {
                DivisionComboBox.Items.Clear();

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



        // Load regions filtered by the selected market
        private void LoadRegions(string marketID)
        {
            try
            {
                RegionComboBox.Items.Clear();

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT RegionID, RegionName FROM Regions WHERE MarketID = @MarketID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MarketID", marketID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = $"{reader["RegionName"]} (ID: {reader["RegionID"]})",
                                    Tag = reader["RegionID"].ToString()
                                };

                                RegionComboBox.Items.Add(item);
                            }
                        }
                    }
                }

                if (RegionComboBox.Items.Count == 0)
                {
                    MessageBox.Show("No regions found for the selected market.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading regions: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load supervisors into the RegionSupervisorComboBox
        private void LoadSupervisors()
        {
            try
            {
                RegionSupervisorComboBox.Items.Clear();

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

                                RegionSupervisorComboBox.Items.Add(item);
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

        // Load region details for the selected region
        private void LoadRegionDetails(string regionID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT RegionName, RegionSupervisorID FROM Regions WHERE RegionID = @RegionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RegionID", regionID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                RegionNameTextBox.Text = reader["RegionName"].ToString();
                                string supervisorID = reader["RegionSupervisorID"].ToString();

                                foreach (ComboBoxItem item in RegionSupervisorComboBox.Items)
                                {
                                    if (item.Tag != null && item.Tag.ToString() == supervisorID)
                                    {
                                        RegionSupervisorComboBox.SelectedItem = item;
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
                MessageBox.Show($"Error loading region details: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Save region changes
        private void SaveRegion_Click(object sender, RoutedEventArgs e)
        {
            if (RegionComboBox.SelectedItem is not ComboBoxItem selectedRegion)
            {
                MessageBox.Show("Please select a region to edit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string regionID = selectedRegion.Tag.ToString();
            string regionName = RegionNameTextBox.Text.Trim();
            string supervisorID = (RegionSupervisorComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

            if (string.IsNullOrEmpty(regionName))
            {
                MessageBox.Show("Region name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "UPDATE Regions SET RegionName = @RegionName, RegionSupervisorID = @SupervisorID WHERE RegionID = @RegionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RegionName", regionName);
                        cmd.Parameters.AddWithValue("@SupervisorID", supervisorID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RegionID", regionID);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Region updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error updating region: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Delete the selected region
        private void DeleteRegion_Click(object sender, RoutedEventArgs e)
        {
            if (RegionComboBox.SelectedItem is not ComboBoxItem selectedRegion)
            {
                MessageBox.Show("Please select a region to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string regionID = selectedRegion.Tag.ToString();

            var result = MessageBox.Show("Are you sure you want to delete this region? This action cannot be undone.", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        string query = "DELETE FROM Regions WHERE RegionID = @RegionID";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@RegionID", regionID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Region deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    RegionComboBox.Items.Remove(selectedRegion);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error deleting region: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void RegionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegionComboBox.SelectedItem is ComboBoxItem selectedRegion)
            {
                string regionID = selectedRegion.Tag.ToString();
                LoadRegionDetails(regionID);
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
                RegionNameTextBox.Clear();
                RegionSupervisorComboBox.SelectedIndex = -1;
            }
        }

        // Handle market selection and populate fields
        private void MarketComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MarketComboBox.SelectedItem is ComboBoxItem selectedMarket)
            {
                string marketID = selectedMarket.Tag.ToString();
                LoadMarketDetails(marketID);
                // Load regions for the selected market
                LoadRegions(marketID);
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
                                RegionSupervisorComboBox.Text = reader["MarketName"].ToString();
                                string supervisorID = reader["MarketSupervisorID"].ToString();

                                foreach (ComboBoxItem item in RegionSupervisorComboBox.Items)
                                {
                                    if (item.Tag != null && item.Tag.ToString() == supervisorID)
                                    {
                                        RegionSupervisorComboBox.SelectedItem = item;
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
    }
}
