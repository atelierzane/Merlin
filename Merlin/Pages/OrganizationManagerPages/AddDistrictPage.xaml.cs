using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class AddDistrictPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private List<Employee> supervisors = new List<Employee>();

        public AddDistrictPage()
        {
            InitializeComponent();
            LoadDropdowns();

            DivisionComboBox.SelectionChanged += DivisionComboBox_SelectionChanged;
            MarketComboBox.SelectionChanged += MarketComboBox_SelectionChanged;
        }

        private void LoadDropdowns()
        {
            try
            {
                LoadSupervisors();

                // Load only Divisions initially
                LoadComboBox("SELECT DivisionID, DivisionName FROM Divisions", DivisionComboBox, "DivisionID", "DivisionName");
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading dropdowns: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSupervisors()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        SELECT EmployeeID, EmployeeFirstName, EmployeeLastName, EmployeeEmail, EmployeePhoneNumber, EmployeeType 
                        FROM Employees 
                        WHERE EmployeeType IN ('Supervisor', 'Manager', 'Administrator')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var employee = new Employee
                            {
                                EmployeeID = reader["EmployeeID"].ToString().Trim(),
                                FirstName = reader["EmployeeFirstName"].ToString().Trim(),
                                LastName = reader["EmployeeLastName"].ToString().Trim(),
                                Email = reader["EmployeeEmail"].ToString().Trim(),
                                PhoneNumber = reader["EmployeePhoneNumber"].ToString().Trim(),
                                EmployeeType = reader["EmployeeType"].ToString().Trim()
                            };

                            supervisors.Add(employee);
                        }
                    }
                }

                DistrictSupervisorComboBox.ItemsSource = supervisors;
                DistrictSupervisorComboBox.DisplayMemberPath = "DisplayName";
                DistrictSupervisorComboBox.SelectedValuePath = "EmployeeID";
                DistrictSupervisorComboBox.IsEditable = false;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading supervisors: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadComboBox(string query, ComboBox comboBox, string valueMember, string displayMember)
        {
            comboBox.Items.Clear();

            using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox.Items.Add(new ComboBoxItem
                        {
                            Content = reader[displayMember].ToString(),
                            Tag = reader[valueMember].ToString()
                        });
                    }
                }
            }
        }

        private void DivisionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MarketComboBox.Items.Clear();
            RegionComboBox.Items.Clear();

            if (DivisionComboBox.SelectedItem is ComboBoxItem selectedDivision)
            {
                string divisionID = selectedDivision.Tag?.ToString();

                if (!string.IsNullOrEmpty(divisionID))
                {
                    try
                    {
                        string query = "SELECT MarketID, MarketName FROM Markets WHERE DivisionID = @DivisionID";

                        using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@DivisionID", divisionID);
                            conn.Open();

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    MarketComboBox.Items.Add(new ComboBoxItem
                                    {
                                        Content = reader["MarketName"].ToString(),
                                        Tag = reader["MarketID"].ToString()
                                    });
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Error loading markets: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MarketComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RegionComboBox.Items.Clear();

            if (MarketComboBox.SelectedItem is ComboBoxItem selectedMarket)
            {
                string marketID = selectedMarket.Tag?.ToString();

                if (!string.IsNullOrEmpty(marketID))
                {
                    try
                    {
                        string query = "SELECT RegionID, RegionName FROM Regions WHERE MarketID = @MarketID";

                        using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@MarketID", marketID);
                            conn.Open();

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    RegionComboBox.Items.Add(new ComboBoxItem
                                    {
                                        Content = reader["RegionName"].ToString(),
                                        Tag = reader["RegionID"].ToString()
                                    });
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Error loading regions: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveDistrict_Click(object sender, RoutedEventArgs e)
        {
            string districtID = DistrictIDTextBox.Text.Trim();
            string districtName = DistrictNameTextBox.Text.Trim();
            string supervisorID = DistrictSupervisorComboBox.SelectedValue?.ToString();
            string regionID = (RegionComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            string marketID = (MarketComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            string divisionID = (DivisionComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            if (string.IsNullOrEmpty(districtID) || districtID.Length != 4)
            {
                MessageBox.Show("District ID must be a 4-digit number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO Districts (DistrictID, DistrictName, DistrictSupervisorID, RegionID, MarketID, DivisionID)
                        VALUES (@DistrictID, @DistrictName, @SupervisorID, @RegionID, @MarketID, @DivisionID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DistrictID", districtID);
                        cmd.Parameters.AddWithValue("@DistrictName", districtName);
                        cmd.Parameters.AddWithValue("@SupervisorID", string.IsNullOrEmpty(supervisorID) ? (object)DBNull.Value : supervisorID);
                        cmd.Parameters.AddWithValue("@RegionID", string.IsNullOrEmpty(regionID) ? (object)DBNull.Value : regionID);
                        cmd.Parameters.AddWithValue("@MarketID", string.IsNullOrEmpty(marketID) ? (object)DBNull.Value : marketID);
                        cmd.Parameters.AddWithValue("@DivisionID", string.IsNullOrEmpty(divisionID) ? (object)DBNull.Value : divisionID);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("District added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
