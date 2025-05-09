using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class EditDivisionPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EditDivisionPage()
        {
            InitializeComponent();
            LoadDivisions();
            LoadSupervisors();
        }

        // Load all divisions into the combo box for selection
        private void LoadDivisions()
        {
            try
            {
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

        // Load supervisors into the combo box for editing
        private void LoadSupervisors()
        {
            try
            {
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

                                DivisionSupervisorComboBox.Items.Add(item);
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

        // Handle division selection and populate fields
        private void DivisionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DivisionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string divisionID = selectedItem.Tag.ToString();
                LoadDivisionDetails(divisionID);
            }
        }

        private void LoadDivisionDetails(string divisionID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT DivisionName, DivisionSupervisorID FROM Divisions WHERE DivisionID = @DivisionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionID", divisionID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                DivisionNameTextBox.Text = reader["DivisionName"].ToString();
                                string supervisorID = reader["DivisionSupervisorID"].ToString();

                                foreach (ComboBoxItem item in DivisionSupervisorComboBox.Items)
                                {
                                    if (item.Tag != null && item.Tag.ToString() == supervisorID)
                                    {
                                        DivisionSupervisorComboBox.SelectedItem = item;
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
                MessageBox.Show($"Error loading division details: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Save changes to the division
        private void SaveDivision_Click(object sender, RoutedEventArgs e)
        {
            if (DivisionComboBox.SelectedItem is not ComboBoxItem selectedDivision)
            {
                MessageBox.Show("Please select a division to edit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string divisionID = selectedDivision.Tag.ToString();
            string divisionName = DivisionNameTextBox.Text.Trim();
            string supervisorID = (DivisionSupervisorComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

            if (string.IsNullOrEmpty(divisionName))
            {
                MessageBox.Show("Division name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "UPDATE Divisions SET DivisionName = @DivisionName, DivisionSupervisorID = @SupervisorID WHERE DivisionID = @DivisionID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DivisionName", divisionName);
                        cmd.Parameters.AddWithValue("@SupervisorID", supervisorID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DivisionID", divisionID);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Division updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error updating division: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Delete the selected division
        private void DeleteDivision_Click(object sender, RoutedEventArgs e)
        {
            if (DivisionComboBox.SelectedItem is not ComboBoxItem selectedDivision)
            {
                MessageBox.Show("Please select a division to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string divisionID = selectedDivision.Tag.ToString();

            var result = MessageBox.Show("Are you sure you want to delete this division? This action cannot be undone.", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        string query = "DELETE FROM Divisions WHERE DivisionID = @DivisionID";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@DivisionID", divisionID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Division deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DivisionComboBox.Items.Remove(selectedDivision); // Remove from the ComboBox
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error deleting division: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
