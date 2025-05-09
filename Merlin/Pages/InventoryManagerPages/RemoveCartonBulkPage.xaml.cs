using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.InventoryManagerPages
{
    public partial class RemoveCartonBulkPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public RemoveCartonBulkPage()
        {
            InitializeComponent();
        }

        // Load cartons based on search criteria
        private void LoadCartons(string cartonID, string status, string origin, string destination)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CartonID, CartonStatus, CartonOrigin, CartonDestination " +
                                   "FROM Cartons WHERE 1=1"; // Allows for additional filtering conditions

                    if (!string.IsNullOrWhiteSpace(cartonID))
                    {
                        query += " AND CartonID = @CartonID";
                    }
                    if (!string.IsNullOrWhiteSpace(status) && status != "All Statuses")
                    {
                        query += " AND CartonStatus = @CartonStatus";
                    }
                    if (!string.IsNullOrWhiteSpace(origin))
                    {
                        query += " AND CartonOrigin LIKE @CartonOrigin";
                    }
                    if (!string.IsNullOrWhiteSpace(destination))
                    {
                        query += " AND CartonDestination LIKE @CartonDestination";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(cartonID))
                            cmd.Parameters.AddWithValue("@CartonID", cartonID);
                        if (!string.IsNullOrWhiteSpace(status) && status != "All Statuses")
                            cmd.Parameters.AddWithValue("@CartonStatus", status);
                        if (!string.IsNullOrWhiteSpace(origin))
                            cmd.Parameters.AddWithValue("@CartonOrigin", $"%{origin}%");
                        if (!string.IsNullOrWhiteSpace(destination))
                            cmd.Parameters.AddWithValue("@CartonDestination", $"%{destination}%");

                        List<Carton> cartons = new List<Carton>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cartons.Add(new Carton
                                {
                                    CartonID = reader["CartonID"].ToString(),
                                    CartonStatus = reader["CartonStatus"].ToString(),
                                    CartonOrigin = reader["CartonOrigin"].ToString(),
                                    CartonDestination = reader["CartonDestination"].ToString(),
                                    IsSelected = false // Initially not selected
                                });
                            }
                        }

                        CartonDataGrid.ItemsSource = cartons;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string cartonID = CartonIDTextBox.Text.Trim();
            string status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content.ToString();
            string origin = OriginTextBox.Text.Trim();
            string destination = DestinationTextBox.Text.Trim();

            LoadCartons(cartonID, status, origin, destination);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            CartonIDTextBox.Clear();
            StatusComboBox.SelectedIndex = 0;
            OriginTextBox.Clear();
            DestinationTextBox.Clear();
            CartonDataGrid.ItemsSource = null; // Clear the grid
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (sender as CheckBox).IsChecked == true;

            // Iterate through the items in the DataGrid
            foreach (var item in CartonDataGrid.Items)
            {
                // Ensure the item is of type Carton before casting
                if (item is Carton carton)
                {
                    carton.IsSelected = isChecked;
                }
            }

            // Refresh the DataGrid to reflect the changes
            CartonDataGrid.Items.Refresh();
        }


        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCartons = new List<Carton>();

            // Iterate through the items in the DataGrid and filter only Carton objects
            foreach (var item in CartonDataGrid.Items)
            {
                if (item is Carton carton && carton.IsSelected)
                {
                    selectedCartons.Add(carton);
                }
            }

            if (selectedCartons.Count == 0)
            {
                MessageBox.Show("Please select at least one carton to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {selectedCartons.Count} carton(s)?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        // Start a transaction to ensure atomicity of both Cartons and CartonDetails deletions
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                foreach (var carton in selectedCartons)
                                {
                                    // Delete carton details from the CartonDetails table
                                    string deleteCartonDetailsQuery = "DELETE FROM CartonDetails WHERE CartonID = @CartonID";
                                    using (SqlCommand deleteDetailsCmd = new SqlCommand(deleteCartonDetailsQuery, conn, transaction))
                                    {
                                        deleteDetailsCmd.Parameters.AddWithValue("@CartonID", carton.CartonID);
                                        deleteDetailsCmd.ExecuteNonQuery();
                                    }

                                    // Delete the carton from the Cartons table
                                    string deleteCartonQuery = "DELETE FROM Cartons WHERE CartonID = @CartonID";
                                    using (SqlCommand deleteCartonCmd = new SqlCommand(deleteCartonQuery, conn, transaction))
                                    {
                                        deleteCartonCmd.Parameters.AddWithValue("@CartonID", carton.CartonID);
                                        deleteCartonCmd.ExecuteNonQuery();
                                    }
                                }

                                // Commit the transaction if all deletions were successful
                                transaction.Commit();
                                MessageBox.Show($"{selectedCartons.Count} carton(s) and their details were deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Refresh the data grid after deletion
                                SearchButton_Click(sender, e);
                            }
                            catch (Exception ex)
                            {
                                // Rollback the transaction if there was an error
                                transaction.Rollback();
                                MessageBox.Show($"Error removing cartons: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
