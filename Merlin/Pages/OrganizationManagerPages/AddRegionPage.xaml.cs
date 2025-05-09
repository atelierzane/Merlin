using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class AddRegionPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private List<Employee> supervisors = new List<Employee>();

        public AddRegionPage()
        {
            InitializeComponent();
            LoadDropdowns();

            DivisionComboBox.SelectionChanged += DivisionComboBox_SelectionChanged;
        }

        private void LoadDropdowns()
        {
            try
            {
                LoadSupervisors();

                // Only load divisions initially
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

                RegionSupervisorComboBox.ItemsSource = supervisors;
                RegionSupervisorComboBox.DisplayMemberPath = "DisplayName";
                RegionSupervisorComboBox.SelectedValuePath = "EmployeeID";
                RegionSupervisorComboBox.IsEditable = false;
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

        private void SaveRegion_Click(object sender, RoutedEventArgs e)
        {
            string regionID = RegionIDTextBox.Text.Trim();
            string regionName = RegionNameTextBox.Text.Trim();
            string supervisorID = RegionSupervisorComboBox.SelectedValue?.ToString();
            string marketID = (MarketComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            string divisionID = (DivisionComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            if (string.IsNullOrEmpty(regionID) || regionID.Length != 4)
            {
                MessageBox.Show("Region ID must be a 4-digit number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO Regions (RegionID, RegionName, RegionSupervisorID, MarketID, DivisionID)
                        VALUES (@RegionID, @RegionName, @SupervisorID, @MarketID, @DivisionID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RegionID", regionID);
                        cmd.Parameters.AddWithValue("@RegionName", regionName);
                        cmd.Parameters.AddWithValue("@SupervisorID", string.IsNullOrEmpty(supervisorID) ? (object)DBNull.Value : supervisorID);
                        cmd.Parameters.AddWithValue("@MarketID", string.IsNullOrEmpty(marketID) ? (object)DBNull.Value : marketID);
                        cmd.Parameters.AddWithValue("@DivisionID", string.IsNullOrEmpty(divisionID) ? (object)DBNull.Value : divisionID);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Region added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
