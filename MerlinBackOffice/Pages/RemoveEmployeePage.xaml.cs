using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinBackOffice.Helpers;

namespace MerlinBackOffice.Pages
{
    public partial class RemoveEmployeePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public RemoveEmployeePage()
        {
            InitializeComponent();
        }

        private void SearchEmployee_Click(object sender, RoutedEventArgs e)
        {
            string employeeID = SearchEmployeeTextBox.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT * FROM Employees WHERE EmployeeID = @EmployeeID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string firstName = reader["EmployeeFirstName"].ToString();
                                string lastName = reader["EmployeeLastName"].ToString();

                                EmployeeDetailsTextBlock.Text = $"Employee: {firstName} {lastName}";
                            }
                            else
                            {
                                EmployeeDetailsTextBlock.Text = "Employee not found.";
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error retrieving employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveEmployee_Click(object sender, RoutedEventArgs e)
        {
            string employeeID = SearchEmployeeTextBox.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Employee removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            EmployeeDetailsTextBlock.Text = string.Empty;
                            SearchEmployeeTextBox.Text = string.Empty;
                        }
                        else
                        {
                            MessageBox.Show("Employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error removing employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
