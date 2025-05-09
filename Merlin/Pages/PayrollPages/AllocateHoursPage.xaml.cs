using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.PayrollPages
{
    public partial class AllocateHoursPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public AllocateHoursPage()
        {
            InitializeComponent();
            LoadLocations(); // Load locations at initialization
        }

        // Load all locations into the LocationComboBox
        private void LoadLocations()
        {
            try
            {
                LocationComboBox.Items.Clear();

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT LocationID, LocationStreetAddress FROM Location";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Content = $"{reader["LocationStreetAddress"]} (ID: {reader["LocationID"]})",
                                    Tag = reader["LocationID"].ToString()
                                };

                                LocationComboBox.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading locations: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAllocatedHours(string locationID, DateTime weekEnding)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                        SELECT LocationPayrollHoursAllocated 
                        FROM LocationPayroll 
                        WHERE LocationID = @LocationID AND LocationPayrollWeekEnding = @WeekEnding";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID);
                        cmd.Parameters.AddWithValue("@WeekEnding", weekEnding);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                AllocatedHoursTextBox.Text = reader["LocationPayrollHoursAllocated"].ToString();
                            }
                            else
                            {
                                ResetFields();
                                MessageBox.Show("No payroll data found for the selected location and week.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading allocated hours: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveHours_Click(object sender, RoutedEventArgs e)
        {
            if (LocationComboBox.SelectedItem is not ComboBoxItem selectedLocation)
            {
                MessageBox.Show("Please select a location.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!WeekSelector.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a week.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string locationID = selectedLocation.Tag.ToString();
            DateTime selectedDate = WeekSelector.SelectedDate.Value;
            DateTime weekEnding = selectedDate.AddDays(6 - (int)selectedDate.DayOfWeek);
            decimal allocatedHours = decimal.TryParse(AllocatedHoursTextBox.Text, out var ah) ? ah : 0;

            if (allocatedHours <= 0)
            {
                MessageBox.Show("Allocated hours must be a positive number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Check if the record exists
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM LocationPayroll 
                        WHERE LocationID = @LocationID AND LocationPayrollWeekEnding = @WeekEnding";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@LocationID", locationID);
                        checkCmd.Parameters.AddWithValue("@WeekEnding", weekEnding);

                        int recordExists = (int)checkCmd.ExecuteScalar();

                        if (recordExists == 0)
                        {
                            // Insert a new record if it doesn't exist
                            string insertQuery = @"
                                INSERT INTO LocationPayroll (LocationID, LocationPayrollHoursAllocated, LocationPayrollWeekEnding)
                                VALUES (@LocationID, @AllocatedHours, @WeekEnding)";

                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@LocationID", locationID);
                                insertCmd.Parameters.AddWithValue("@AllocatedHours", allocatedHours);
                                insertCmd.Parameters.AddWithValue("@WeekEnding", weekEnding);

                                insertCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // Update the existing record
                            string updateQuery = @"
                                UPDATE LocationPayroll
                                SET LocationPayrollHoursAllocated = @AllocatedHours
                                WHERE LocationID = @LocationID AND LocationPayrollWeekEnding = @WeekEnding";

                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@LocationID", locationID);
                                updateCmd.Parameters.AddWithValue("@AllocatedHours", allocatedHours);
                                updateCmd.Parameters.AddWithValue("@WeekEnding", weekEnding);

                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                MessageBox.Show("Allocated payroll hours updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error saving allocated hours: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFields_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
        }

        private void ResetFields()
        {
            AllocatedHoursTextBox.Clear();
        }

        private void LocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocationComboBox.SelectedItem is ComboBoxItem selectedLocation && WeekSelector.SelectedDate.HasValue)
            {
                string locationID = selectedLocation.Tag.ToString();
                DateTime selectedDate = WeekSelector.SelectedDate.Value;
                DateTime weekEnding = selectedDate.AddDays(6 - (int)selectedDate.DayOfWeek);
                LoadAllocatedHours(locationID, weekEnding);
            }
        }

        private void WeekSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!WeekSelector.SelectedDate.HasValue)
                return;

            DateTime selectedDate = WeekSelector.SelectedDate.Value;
            DateTime weekEnding = selectedDate.AddDays(6 - (int)selectedDate.DayOfWeek); // Saturday

            WeekEndingDisplay.Text = $"Week Ending: {weekEnding.ToShortDateString()}";

            if (LocationComboBox.SelectedItem is ComboBoxItem selectedLocation)
            {
                string locationID = selectedLocation.Tag.ToString();
                LoadAllocatedHours(locationID, weekEnding);
            }
        }

    }
}
