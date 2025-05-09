using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class EditDistrictPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EditDistrictPage()
        {
            InitializeComponent();
            LoadDivisions();
            LoadSupervisors();
        }

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

        private void LoadMarkets(string divisionID)
        {
            try
            {
                MarketComboBox.Items.Clear();

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT MarketID, MarketName FROM Markets WHERE DivisionID = @DivisionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionID", divisionID);

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
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading markets: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading regions: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDistricts(string regionID)
        {
            try
            {
                DistrictComboBox.Items.Clear();

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT DistrictID, DistrictName FROM Districts WHERE RegionID = @RegionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RegionID", regionID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = $"{reader["DistrictName"]} (ID: {reader["DistrictID"]})",
                                    Tag = reader["DistrictID"].ToString()
                                };

                                DistrictComboBox.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading districts: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSupervisors()
        {
            try
            {
                DistrictSupervisorComboBox.Items.Clear();

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

                                DistrictSupervisorComboBox.Items.Add(item);
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
                                DistrictSupervisorComboBox.Text = reader["MarketName"].ToString();
                                string supervisorID = reader["MarketSupervisorID"].ToString();

                                foreach (ComboBoxItem item in DistrictSupervisorComboBox.Items)
                                {
                                    if (item.Tag != null && item.Tag.ToString() == supervisorID)
                                    {
                                        DistrictSupervisorComboBox.SelectedItem = item;
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
                                DistrictNameTextBox.Text = reader["RegionName"].ToString();
                                string supervisorID = reader["RegionSupervisorID"].ToString();

                                foreach (ComboBoxItem item in DistrictSupervisorComboBox.Items)
                                {
                                    if (item.Tag != null && item.Tag.ToString() == supervisorID)
                                    {
                                        DistrictSupervisorComboBox.SelectedItem = item;
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

        // Handle division selection to load markets for the selected division
        private void DivisionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DivisionComboBox.SelectedItem is ComboBoxItem selectedDivision)
            {
                string divisionID = selectedDivision.Tag.ToString();

                // Load markets for the selected division
                LoadMarkets(divisionID);

                // Clear fields related to market details
                DistrictNameTextBox.Clear();
                DistrictSupervisorComboBox.SelectedIndex = -1;
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
        private void RegionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RegionComboBox.SelectedItem is ComboBoxItem selectedRegion)
            {
                string regionID = selectedRegion.Tag.ToString();
                LoadRegionDetails(regionID);
            }
        }

        private void SaveDistrict_Click(object sender, RoutedEventArgs e)
        {
            // Save logic for the district
        }

        private void DeleteDistrict_Click(object sender, RoutedEventArgs e)
        {
            // Delete logic for the district
        }

        private void DistrictComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle district selection changes
        }
    }
}
