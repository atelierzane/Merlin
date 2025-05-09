using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinBackOffice.Helpers;
using MerlinBackOffice.Models;

namespace MerlinBackOffice.Windows.InventoryWindows
{
    public partial class CountHistoryWindow : Window
    {
        private ObservableCollection<Count> counts;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();

        public CountHistoryWindow()
        {
            InitializeComponent();
            LoadCounts();
        }

        private void LoadCounts()
        {
            counts = new ObservableCollection<Count>();

            string query = @"
        SELECT 
            c.CountID, 
            c.CountType, 
            c.CountCategory, 
            c.CountCompletionDate, 
            c.CountEmployeeIDCompleted, 
            ISNULL(e.EmployeeID, c.CountEmployeeIDCompleted) AS CompletedByEmployee, 
            c.CountAccuracy
        FROM Counts c
        LEFT JOIN Employees e ON c.CountEmployeeIDCompleted = e.EmployeeID
        ORDER BY c.CountCompletionDate DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Ensure all fields are read and assigned properly
                                counts.Add(new Count
                                {
                                    CountID = reader["CountID"].ToString(),
                                    CountType = reader["CountType"].ToString(),
                                    CountCategory = reader["CountCategory"] == DBNull.Value
                                        ? "N/A"
                                        : reader["CountCategory"].ToString(), // Handle NULL for CountCategory
                                    CompletedDate = reader["CountCompletionDate"] == DBNull.Value
                                        ? DateTime.MinValue
                                        : Convert.ToDateTime(reader["CountCompletionDate"]), // Ensure date is valid
                                    CompletedByEmployee = reader["CompletedByEmployee"].ToString(), // Use EmployeeName or fallback to CountEmployeeIDCompleted
                                    Accuracy = reader["CountAccuracy"] == DBNull.Value
                                        ? 0.0m
                                        : Convert.ToDecimal(reader["CountAccuracy"]) // Ensure accuracy is set
                                });
                            }
                        }
                    }
                }

                lvCounts.ItemsSource = counts;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading counts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidCastException ex)
            {
                MessageBox.Show($"Data conversion error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnViewCountDetails_Click(object sender, RoutedEventArgs e)
        {
            if (lvCounts.SelectedItem is Count selectedCount)
            {
                string countID = selectedCount.CountID;

                CountDetailsWindow detailsWindow = new CountDetailsWindow(countID);
                detailsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a count to view details.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void lvCounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional logic when a count is selected.
        }
    }
}
