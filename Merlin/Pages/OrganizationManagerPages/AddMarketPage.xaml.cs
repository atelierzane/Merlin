using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class AddMarketPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private List<Employee> supervisors = new List<Employee>();

        public AddMarketPage()
        {
            InitializeComponent();
            LoadDropdowns();
        }

        private void LoadDropdowns()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Load Supervisors (Employee binding)
                    LoadSupervisors();

                    // Load Divisions
                    LoadComboBox("SELECT DivisionID, DivisionName FROM Divisions", DivisionComboBox, "DivisionID", "DivisionName");
                }
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
                    string query = "SELECT EmployeeID, EmployeeFirstName, EmployeeLastName, EmployeeEmail, EmployeePhoneNumber, EmployeeType " +
                                   "FROM Employees WHERE EmployeeType IN ('Supervisor', 'Manager', 'Administrator')";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
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
                }

                MarketSupervisorComboBox.ItemsSource = supervisors;
                MarketSupervisorComboBox.DisplayMemberPath = "DisplayName";
                MarketSupervisorComboBox.SelectedValuePath = "EmployeeID";
                MarketSupervisorComboBox.IsEditable = false;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading supervisors: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadComboBox(string query, ComboBox comboBox, string valueMember, string displayMember)
        {
            using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
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

        private void SaveMarket_Click(object sender, RoutedEventArgs e)
        {
            string marketID = MarketIDTextBox.Text.Trim();
            string marketName = MarketNameTextBox.Text.Trim();
            string supervisorID = MarketSupervisorComboBox.SelectedValue as string;
            string divisionID = (DivisionComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

            if (string.IsNullOrEmpty(marketID) || marketID.Length != 4)
            {
                MessageBox.Show("Market ID must be a 4-digit number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "INSERT INTO Markets (MarketID, MarketName, MarketSupervisorID, DivisionID) " +
                                   "VALUES (@MarketID, @MarketName, @SupervisorID, @DivisionID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MarketID", marketID);
                        cmd.Parameters.AddWithValue("@MarketName", marketName);
                        cmd.Parameters.AddWithValue("@SupervisorID", supervisorID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DivisionID", divisionID ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Market added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
