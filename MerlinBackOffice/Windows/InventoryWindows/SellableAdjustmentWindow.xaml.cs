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
    public partial class SellableAdjustmentWindow : Window
    {
        private ObservableCollection<AdjustmentItem> pendingAdjustments = new ObservableCollection<AdjustmentItem>();
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public SellableAdjustmentWindow()
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
            string locationID = Properties.Settings.Default.LocationID; // Get the current LocationID

            // First, check if the SKU exists in the Catalog
            string catalogQuery = "SELECT ProductName, CategoryID FROM Catalog WHERE SKU = @SKU";

            // Then, check if there's an entry for this SKU in the Inventory for this LocationID
            string inventoryQuery = @"
                                    SELECT QuantityOnHandSellable, QuantityOnHandDefective 
                                    FROM Inventory 
                                    WHERE SKU = @SKU AND LocationID = @LocationID";

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Step 1: Check if the SKU exists in the Catalog
                    using (SqlCommand catalogCmd = new SqlCommand(catalogQuery, conn))
                    {
                        catalogCmd.Parameters.AddWithValue("@SKU", sku);

                        using (SqlDataReader catalogReader = catalogCmd.ExecuteReader())
                        {
                            if (catalogReader.Read())
                            {
                                // SKU found in Catalog, populate product info
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

                    // Step 2: Check if the SKU exists in the Inventory for this location
                    using (SqlCommand inventoryCmd = new SqlCommand(inventoryQuery, conn))
                    {
                        inventoryCmd.Parameters.AddWithValue("@SKU", sku);
                        inventoryCmd.Parameters.AddWithValue("@LocationID", locationID);

                        using (SqlDataReader inventoryReader = inventoryCmd.ExecuteReader())
                        {
                            if (inventoryReader.Read())
                            {
                                // SKU exists in Inventory for this location, populate quantities
                                txtQuantitySellable.Text = inventoryReader["QuantityOnHandSellable"].ToString();
                                txtQuantityDefective.Text = inventoryReader["QuantityOnHandDefective"].ToString();
                            }
                            else
                            {
                                // SKU not found in Inventory for this location, assume quantities are zero
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
                MessageBox.Show("Base SKUs cannot be adjusted in inventory.", "Invalid SKU", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string adjustmentType = ((ComboBoxItem)cmbAdjustmentType.SelectedItem).Content.ToString();
            int originalQuantity = Convert.ToInt32(txtQuantitySellable.Text);
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
            string locationID = Properties.Settings.Default.LocationID; // Get the current LocationID

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
                            MessageBox.Show($"Base SKU {adjustment.SKU} is omitted from adjustments.", "Omitted SKU", MessageBoxButton.OK, MessageBoxImage.Information);
                            continue; // Skip this adjustment
                        }

                        // Check if the SKU and LocationID already exist in the Inventory
                        string checkQuery = "SELECT COUNT(*) FROM Inventory WHERE SKU = @SKU AND LocationID = @LocationID";

                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@SKU", adjustment.SKU);
                            checkCmd.Parameters.AddWithValue("@LocationID", locationID);

                            int count = (int)checkCmd.ExecuteScalar();

                            if (count > 0)
                            {
                                // SKU and LocationID exist, so we update the record
                                string updateQuery = "UPDATE Inventory SET QuantityOnHandSellable = @NewQuantitySellable, " +
                                                     "QuantityOnHandDefective = @NewQuantityDefective, CategoryID = @CategoryID " +
                                                     "WHERE SKU = @SKU AND LocationID = @LocationID";

                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@NewQuantitySellable", adjustment.NewQuantity);
                                    updateCmd.Parameters.AddWithValue("@NewQuantityDefective", 0); // Set default to 0 for now, or adjust as necessary
                                    updateCmd.Parameters.AddWithValue("@CategoryID", adjustment.CategoryID); // Ensure CategoryID is updated
                                    updateCmd.Parameters.AddWithValue("@SKU", adjustment.SKU);
                                    updateCmd.Parameters.AddWithValue("@LocationID", locationID);

                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // SKU and LocationID do not exist, so we insert a new record
                                string insertQuery = "INSERT INTO Inventory (SKU, LocationID, CategoryID, QuantityOnHandSellable, QuantityOnHandDefective) " +
                                                     "VALUES (@SKU, @LocationID, @CategoryID, @NewQuantitySellable, @NewQuantityDefective)";

                                using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@SKU", adjustment.SKU);
                                    insertCmd.Parameters.AddWithValue("@LocationID", locationID);
                                    insertCmd.Parameters.AddWithValue("@CategoryID", adjustment.CategoryID); // Ensure CategoryID is inserted
                                    insertCmd.Parameters.AddWithValue("@NewQuantitySellable", adjustment.NewQuantity);
                                    insertCmd.Parameters.AddWithValue("@NewQuantityDefective", 0); // Set default to 0, or adjust as necessary

                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // Clear the pending adjustments and notify the user
                    pendingAdjustments.Clear();
                    MessageBox.Show("All adjustments have been finalized, excluding Base SKUs.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error finalizing adjustments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    // Query to check if the SKU is a BaseSKU
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


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Placeholder handling: Clear the text when focusing if it contains placeholder
            TextBox textBox = sender as TextBox;
            if (textBox != null && (textBox.Text == "Enter SKU" || textBox.Text == "New Quantity"))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black; // Set text color back to normal
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Placeholder handling: Restore placeholder text if the TextBox is empty
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
                textBox.Foreground = Brushes.Gray; // Set text color to indicate placeholder
            }
        }

    }

}
