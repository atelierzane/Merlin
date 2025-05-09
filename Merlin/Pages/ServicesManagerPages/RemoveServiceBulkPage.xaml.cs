using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.ServicesManagerPages
{
    public partial class RemoveServiceBulkPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public RemoveServiceBulkPage()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string serviceID = ServiceIDTextBox.Text.Trim();
            string serviceName = ServiceNameTextBox.Text.Trim();
            string addOnID = AddOnIDTextBox.Text.Trim();
            string feeID = FeeIDTextBox.Text.Trim();

            LoadServices(serviceID, serviceName, addOnID, feeID);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ServiceIDTextBox.Clear();
            ServiceNameTextBox.Clear();
            AddOnIDTextBox.Clear();
            FeeIDTextBox.Clear();
            ServiceDataGrid.ItemsSource = null; // Clear the DataGrid
        }

        private void LoadServices(string serviceID, string serviceName, string addOnID, string feeID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                        SELECT ServiceID AS ID, ServiceName AS Name, 'Service' AS Type, ServicePrice AS Price
                        FROM Services
                        WHERE (@ServiceID IS NULL OR ServiceID = @ServiceID)
                          AND (@ServiceName IS NULL OR ServiceName LIKE @ServiceName)

                        UNION ALL

                        SELECT ServicePlusID AS ID, ServicePlusName AS Name, 'Add-On' AS Type, ServicePlusPrice AS Price
                        FROM ServiceAddOns
                        WHERE (@AddOnID IS NULL OR ServicePlusID = @AddOnID)

                        UNION ALL

                        SELECT ServiceFeeID AS ID, ServiceFeeName AS Name, 'Fee' AS Type, ServiceFeePrice AS Price
                        FROM ServiceFees
                        WHERE (@FeeID IS NULL OR ServiceFeeID = @FeeID)
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceID", string.IsNullOrWhiteSpace(serviceID) ? (object)DBNull.Value : serviceID);
                        cmd.Parameters.AddWithValue("@ServiceName", string.IsNullOrWhiteSpace(serviceName) ? (object)DBNull.Value : $"%{serviceName}%");
                        cmd.Parameters.AddWithValue("@AddOnID", string.IsNullOrWhiteSpace(addOnID) ? (object)DBNull.Value : addOnID);
                        cmd.Parameters.AddWithValue("@FeeID", string.IsNullOrWhiteSpace(feeID) ? (object)DBNull.Value : feeID);

                        List<ServiceResult> results = new List<ServiceResult>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new ServiceResult
                                {
                                    ID = reader["ID"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Type = reader["Type"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    IsSelected = false
                                });
                            }
                        }

                        ServiceDataGrid.ItemsSource = results;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = new List<ServiceResult>();

            foreach (var item in ServiceDataGrid.Items)
            {
                if (item is ServiceResult result && result.IsSelected)
                {
                    selectedItems.Add(result);
                }
            }

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one item to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirmation = MessageBox.Show($"Are you sure you want to delete {selectedItems.Count} item(s)?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmation == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();

                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                foreach (var item in selectedItems)
                                {
                                    string deleteQuery = item.Type switch
                                    {
                                        "Service" => "DELETE FROM Services WHERE ServiceID = @ID",
                                        "Add-On" => "DELETE FROM ServiceAddOns WHERE ServicePlusID = @ID",
                                        "Fee" => "DELETE FROM ServiceFees WHERE ServiceFeeID = @ID",
                                        _ => throw new InvalidOperationException("Unknown type")
                                    };

                                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@ID", item.ID);
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                                MessageBox.Show($"{selectedItems.Count} item(s) deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                SearchButton_Click(sender, e);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Error deleting items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (sender as CheckBox)?.IsChecked == true;

            foreach (var item in ServiceDataGrid.Items)
            {
                if (item is ServiceResult result)
                {
                    result.IsSelected = isChecked;
                }
            }

            ServiceDataGrid.Items.Refresh();
        }

        private class ServiceResult
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public decimal Price { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
