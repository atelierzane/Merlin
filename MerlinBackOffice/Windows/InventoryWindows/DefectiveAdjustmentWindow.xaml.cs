using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MerlinBackOffice.Helpers;
using MerlinBackOffice.Models;

namespace MerlinBackOffice.Windows.InventoryWindows
{
    public partial class DefectiveAdjustmentWindow : Window
    {
        private ObservableCollection<AdjustmentItem> pendingAdjustments = new ObservableCollection<AdjustmentItem>();
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public DefectiveAdjustmentWindow()
        {
            InitializeComponent();
            ResetWindow();
        }

        private void ResetWindow()
        {
            pendingAdjustments.Clear();
            dgPendingAdjustments.ItemsSource = pendingAdjustments;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string sku = txtSearchSKU.Text.Trim();
            string locationID = Properties.Settings.Default.LocationID;

            string catalogQuery = "SELECT ProductName, CategoryID FROM Catalog WHERE SKU = @SKU";
            string inventoryQuery = @"
                SELECT QuantityOnHandSellable, QuantityOnHandDefective 
                FROM Inventory 
                WHERE SKU = @SKU AND LocationID = @LocationID";

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    using (SqlCommand catalogCmd = new SqlCommand(catalogQuery, conn))
                    {
                        catalogCmd.Parameters.AddWithValue("@SKU", sku);

                        using (SqlDataReader catalogReader = catalogCmd.ExecuteReader())
                        {
                            if (catalogReader.Read())
                            {
                                txtProductName.Text = catalogReader["ProductName"].ToString();
                                txtCategory.Text = catalogReader["CategoryID"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("SKU not found in catalog.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }
                    }

                    using (SqlCommand inventoryCmd = new SqlCommand(inventoryQuery, conn))
                    {
                        inventoryCmd.Parameters.AddWithValue("@SKU", sku);
                        inventoryCmd.Parameters.AddWithValue("@LocationID", locationID);

                        using (SqlDataReader inventoryReader = inventoryCmd.ExecuteReader())
                        {
                            if (inventoryReader.Read())
                            {
                                txtQuantitySellable.Text = inventoryReader["QuantityOnHandSellable"].ToString();
                                txtQuantityDefective.Text = inventoryReader["QuantityOnHandDefective"].ToString();
                            }
                            else
                            {
                                txtQuantitySellable.Text = "0";
                                txtQuantityDefective.Text = "0";
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

        private bool IsBaseSKU(string sku)
        {
            bool isBaseSKU = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT IsBaseSKU FROM Catalog WHERE SKU = @SKU";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SKU", sku);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            isBaseSKU = Convert.ToBoolean(result);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error while checking BaseSKU: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return isBaseSKU;
        }

        private void AddAdjustment_Click(object sender, RoutedEventArgs e)
        {
            string sku = txtSearchSKU.Text.Trim();

            if (string.IsNullOrWhiteSpace(sku))
            {
                MessageBox.Show("Please enter a valid SKU.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if the SKU is a BaseSKU
            if (IsBaseSKU(sku))
            {
                MessageBox.Show("Base SKUs cannot be adjusted in defective inventory.", "Invalid SKU", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string adjustmentType = ((ComboBoxItem)cmbAdjustmentType.SelectedItem).Content.ToString();
            int originalQuantity = Convert.ToInt32(txtQuantityDefective.Text);
            int newQuantity = Convert.ToInt32(txtNewQuantity.Text);
            int quantityDifference = newQuantity - originalQuantity;

            pendingAdjustments.Add(new AdjustmentItem
            {
                SKU = sku,
                ProductName = txtProductName.Text,
                CategoryID = txtCategory.Text,
                AdjustmentType = adjustmentType,
                OriginalQuantity = originalQuantity,
                NewQuantity = newQuantity,
                QuantityDifference = quantityDifference
            });
        }

        private void FinalizeAdjustments_Click(object sender, RoutedEventArgs e)
        {
            string locationID = Properties.Settings.Default.LocationID;

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    foreach (var adjustment in pendingAdjustments)
                    {
                        // Check if the SKU is a BaseSKU and skip if true
                        if (IsBaseSKU(adjustment.SKU))
                        {
                            MessageBox.Show($"Base SKU {adjustment.SKU} is omitted from defective adjustments.", "Omitted SKU", MessageBoxButton.OK, MessageBoxImage.Information);
                            continue; // Skip this adjustment
                        }

                        int quantityDifference = adjustment.QuantityDifference;
                        string adjustmentType = adjustment.AdjustmentType;
                        string updateQuery = string.Empty;

                        if (adjustmentType == "To Defective")
                        {
                            updateQuery = @"
                    UPDATE Inventory
                    SET
                        QuantityOnHandDefective = QuantityOnHandDefective + @QuantityDifference,
                        QuantityOnHandSellable = QuantityOnHandSellable - @QuantityDifference
                    WHERE SKU = @SKU AND LocationID = @LocationID";
                        }
                        else if (adjustmentType == "From Defective")
                        {
                            updateQuery = @"
                    UPDATE Inventory
                    SET
                        QuantityOnHandDefective = QuantityOnHandDefective - @QuantityDifference,
                        QuantityOnHandSellable = QuantityOnHandSellable + @QuantityDifference
                    WHERE SKU = @SKU AND LocationID = @LocationID";
                        }

                        if (string.IsNullOrWhiteSpace(updateQuery))
                        {
                            MessageBox.Show($"Invalid adjustment type for SKU {adjustment.SKU}. Please check the adjustment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                        {
                            updateCmd.Parameters.AddWithValue("@QuantityDifference", Math.Abs(quantityDifference));
                            updateCmd.Parameters.AddWithValue("@SKU", adjustment.SKU);
                            updateCmd.Parameters.AddWithValue("@LocationID", locationID);

                            int rowsAffected = updateCmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                MessageBox.Show($"Failed to update inventory for SKU {adjustment.SKU}. Ensure the SKU exists for this location.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                continue;
                            }
                        }
                    }

                    pendingAdjustments.Clear();
                    MessageBox.Show("All defective adjustments have been finalized, excluding Base SKUs.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error finalizing defective adjustments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && (textBox.Text == "Enter SKU" || textBox.Text == "New Quantity"))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "txtSearchSKU")
                {
                    textBox.Text = "Enter SKU";
                }
                else if (textBox.Name == "txtNewQuantity")
                {
                    textBox.Text = "New Quantity";
                }
                textBox.Foreground = Brushes.Gray;
            }
        }
    }
}
