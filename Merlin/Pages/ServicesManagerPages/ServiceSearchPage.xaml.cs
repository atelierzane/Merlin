using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.ServicesManagerPages
{
    public partial class ServiceSearchPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public ServiceSearchPage()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string serviceID = ServiceIDTextBox.Text.Trim();
            string serviceName = ServiceNameTextBox.Text.Trim();
            string minPriceText = MinPriceTextBox.Text.Trim();
            string maxPriceText = MaxPriceTextBox.Text.Trim();
            string addOnID = AddOnIDTextBox.Text.Trim();
            string addOnName = AddOnNameTextBox.Text.Trim();
            string feeID = FeeIDTextBox.Text.Trim();
            string feeName = FeeNameTextBox.Text.Trim();

            decimal? minPrice = null, maxPrice = null;

            // Try parsing price filters
            if (decimal.TryParse(minPriceText, out decimal parsedMinPrice))
                minPrice = parsedMinPrice;
            if (decimal.TryParse(maxPriceText, out decimal parsedMaxPrice))
                maxPrice = parsedMaxPrice;

            LoadServices(serviceID, serviceName, minPrice, maxPrice, addOnID, addOnName, feeID, feeName);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset all filters
            ServiceIDTextBox.Clear();
            ServiceNameTextBox.Clear();
            MinPriceTextBox.Clear();
            MaxPriceTextBox.Clear();
            AddOnIDTextBox.Clear();
            AddOnNameTextBox.Clear();
            FeeIDTextBox.Clear();
            FeeNameTextBox.Clear();
            ServiceDataGrid.ItemsSource = null; // Clear the DataGrid
        }

        private void LoadServices(string serviceID, string serviceName, decimal? minPrice, decimal? maxPrice, string addOnID, string addOnName, string feeID, string feeName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                        SELECT ServiceID AS ID, NULL AS ServiceID, ServiceName AS Name, 'Service' AS Type, ServicePrice AS Price, ServicePricedBy AS PricedBy
                        FROM Services
                        WHERE (@ServiceID IS NULL OR ServiceID = @ServiceID)
                          AND (@ServiceName IS NULL OR ServiceName LIKE @ServiceName)
                          AND (@MinPrice IS NULL OR ServicePrice >= @MinPrice)
                          AND (@MaxPrice IS NULL OR ServicePrice <= @MaxPrice)

                        UNION ALL

                        SELECT ServicePlusID AS ID, ServiceID, ServicePlusName AS Name, 'Add-On' AS Type, ServicePlusPrice AS Price, ServicePlusPricedBy AS PricedBy
                        FROM ServiceAddOns
                        WHERE (@ServiceID IS NULL OR ServiceID = @ServiceID)
                          AND (@AddOnID IS NULL OR ServicePlusID = @AddOnID)
                          AND (@AddOnName IS NULL OR ServicePlusName LIKE @AddOnName)

                        UNION ALL

                        SELECT ServiceFeeID AS ID, ServiceID, ServiceFeeName AS Name, 'Fee' AS Type, ServiceFeePrice AS Price, ServiceFeePricedBy AS PricedBy
                        FROM ServiceFees
                        WHERE (@ServiceID IS NULL OR ServiceID = @ServiceID)
                          AND (@FeeID IS NULL OR ServiceFeeID = @FeeID)
                          AND (@FeeName IS NULL OR ServiceFeeName LIKE @FeeName)
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceID", string.IsNullOrWhiteSpace(serviceID) ? (object)DBNull.Value : serviceID);
                        cmd.Parameters.AddWithValue("@ServiceName", string.IsNullOrWhiteSpace(serviceName) ? (object)DBNull.Value : $"%{serviceName}%");
                        cmd.Parameters.AddWithValue("@MinPrice", minPrice.HasValue ? (object)minPrice.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxPrice", maxPrice.HasValue ? (object)maxPrice.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@AddOnID", string.IsNullOrWhiteSpace(addOnID) ? (object)DBNull.Value : addOnID);
                        cmd.Parameters.AddWithValue("@AddOnName", string.IsNullOrWhiteSpace(addOnName) ? (object)DBNull.Value : $"%{addOnName}%");
                        cmd.Parameters.AddWithValue("@FeeID", string.IsNullOrWhiteSpace(feeID) ? (object)DBNull.Value : feeID);
                        cmd.Parameters.AddWithValue("@FeeName", string.IsNullOrWhiteSpace(feeName) ? (object)DBNull.Value : $"%{feeName}%");

                        List<ServiceResult> results = new List<ServiceResult>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new ServiceResult
                                {
                                    ID = reader["ID"].ToString(),
                                    ServiceID = reader["ServiceID"] as string ?? string.Empty, // ServiceID is NULL for main services
                                    Name = reader["Name"].ToString(),
                                    Type = reader["Type"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    PricedBy = reader["PricedBy"].ToString()
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

        private class ServiceResult
        {
            public string ID { get; set; } // ServiceID, Add-On ID, or Fee ID
            public string ServiceID { get; set; } // The associated ServiceID (if applicable)
            public string Name { get; set; }
            public string Type { get; set; } // "Service", "Add-On", or "Fee"
            public decimal Price { get; set; }
            public string PricedBy { get; set; }
        }
    }
}
