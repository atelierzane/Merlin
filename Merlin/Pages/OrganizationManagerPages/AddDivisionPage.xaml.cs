using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class AddDivisionPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private List<Employee> supervisors = new List<Employee>();

        public AddDivisionPage()
        {
            InitializeComponent();
            LoadSupervisors();
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

                DivisionSupervisorComboBox.ItemsSource = supervisors;
                DivisionSupervisorComboBox.DisplayMemberPath = "DisplayName";
                DivisionSupervisorComboBox.SelectedValuePath = "EmployeeID";
                DivisionSupervisorComboBox.IsEditable = false;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading supervisors: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveDivision_Click(object sender, RoutedEventArgs e)
        {
            string divisionID = DivisionIDTextBox.Text.Trim();
            string divisionName = DivisionNameTextBox.Text.Trim();
            string supervisorID = DivisionSupervisorComboBox.SelectedValue?.ToString();

            if (string.IsNullOrEmpty(divisionID) || divisionID.Length != 4)
            {
                MessageBox.Show("Division ID must be a 4-digit number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "INSERT INTO Divisions (DivisionID, DivisionName, DivisionSupervisorID) " +
                                   "VALUES (@DivisionID, @DivisionName, @SupervisorID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionID", divisionID);
                        cmd.Parameters.AddWithValue("@DivisionName", divisionName);
                        cmd.Parameters.AddWithValue("@SupervisorID", supervisorID ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Division added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
