using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.EmployeeManagerPages
{
    public partial class RemoveEmployeePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public RemoveEmployeePage()
        {
            InitializeComponent();
        }

        // Search for the employee by EmployeeID
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string employeeID = EmployeeIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(employeeID))
            {
                MessageBox.Show("Please enter an Employee ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT EmployeeFirstName, EmployeeLastName, EmployeeEmail, EmployeePhoneNumber FROM Employees WHERE EmployeeID = @EmployeeID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                // Populate the employee information fields
                                FirstNameTextBlock.Text = reader["EmployeeFirstName"].ToString();
                                LastNameTextBlock.Text = reader["EmployeeLastName"].ToString();
                                EmailTextBlock.Text = reader["EmployeeEmail"].ToString();
                                PhoneNumberTextBlock.Text = reader["EmployeePhoneNumber"].ToString();

                                // Show the employee information section
                                EmployeeInfoSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No employee found with the given Employee ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                EmployeeInfoSection.Visibility = Visibility.Collapsed;
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

        // Delete the employee
        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            string employeeID = EmployeeIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(employeeID))
            {
                MessageBox.Show("Please search for an employee first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete Employee ID: {employeeID}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Employee deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                // Clear the fields
                                EmployeeIDTextBox.Clear();
                                FirstNameTextBlock.Text = string.Empty;
                                LastNameTextBlock.Text = string.Empty;
                                EmailTextBlock.Text = string.Empty;
                                PhoneNumberTextBlock.Text = string.Empty;
                                EmployeeInfoSection.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete the employee. Employee ID not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
