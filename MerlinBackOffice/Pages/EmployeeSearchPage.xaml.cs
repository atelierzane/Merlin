using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinBackOffice.Models;
using MerlinBackOffice.Helpers;

namespace MerlinBackOffice.Pages
{
    /// <summary>
    /// Interaction logic for EmployeeSearchPage.xaml
    /// </summary>
    public partial class EmployeeSearchPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EmployeeSearchPage()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Collect filter criteria
            string employeeID = EmployeeIDTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phoneNumber = PhoneNumberTextBox.Text.Trim();

            // Load employees based on search criteria
            LoadEmployees(employeeID, firstName, lastName, email, phoneNumber);
        }

        private void LoadEmployees(string employeeID, string firstName, string lastName, string email, string phoneNumber)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"SELECT EmployeeID, EmployeeFirstName, EmployeeLastName, EmployeeEmail, EmployeePhoneNumber, EmployeeType 
                                     FROM Employees WHERE 1=1";

                    // Apply filters dynamically
                    if (!string.IsNullOrWhiteSpace(employeeID))
                        query += " AND EmployeeID = @EmployeeID";
                    if (!string.IsNullOrWhiteSpace(firstName))
                        query += " AND EmployeeFirstName LIKE @FirstName";
                    if (!string.IsNullOrWhiteSpace(lastName))
                        query += " AND EmployeeLastName LIKE @LastName";
                    if (!string.IsNullOrWhiteSpace(email))
                        query += " AND EmployeeEmail LIKE @Email";
                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                        query += " AND EmployeePhoneNumber LIKE @PhoneNumber";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters to prevent SQL injection
                        if (!string.IsNullOrWhiteSpace(employeeID))
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        if (!string.IsNullOrWhiteSpace(firstName))
                            cmd.Parameters.AddWithValue("@FirstName", $"%{firstName}%");
                        if (!string.IsNullOrWhiteSpace(lastName))
                            cmd.Parameters.AddWithValue("@LastName", $"%{lastName}%");
                        if (!string.IsNullOrWhiteSpace(email))
                            cmd.Parameters.AddWithValue("@Email", $"%{email}%");
                        if (!string.IsNullOrWhiteSpace(phoneNumber))
                            cmd.Parameters.AddWithValue("@PhoneNumber", $"%{phoneNumber}%");

                        List<Employee> employees = new List<Employee>();

                        // Execute query and read data
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                employees.Add(new Employee
                                {
                                    EmployeeID = reader["EmployeeID"].ToString(),
                                    FirstName = reader["EmployeeFirstName"].ToString(),
                                    LastName = reader["EmployeeLastName"].ToString(),
                                    Email = reader["EmployeeEmail"].ToString(),
                                    PhoneNumber = reader["EmployeePhoneNumber"].ToString(),
                                    EmployeeType = reader["EmployeeType"].ToString()
                                });
                            }
                        }

                        // Bind the result to the DataGrid
                        EmployeeDataGrid.ItemsSource = employees;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all input fields
            EmployeeIDTextBox.Clear();
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            EmailTextBox.Clear();
            PhoneNumberTextBox.Clear();

            // Clear the DataGrid
            EmployeeDataGrid.ItemsSource = null;
        }

        private void PrintReportButton_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for print functionality
            MessageBox.Show("Print Report functionality is not implemented yet.",
                            "Print Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
