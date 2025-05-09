using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MerlinBackOffice.Helpers;
using MerlinBackOffice.Models;


namespace MerlinBackOffice.Windows.InventoryWindows
{
    public partial class CategoryCountWindow : Window
    {
        private ObservableCollection<Category> categories;
        private ObservableCollection<CountItem> catCountItems;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();
        private bool isCountInProgress = false;

        public CategoryCountWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            categories = new ObservableCollection<Category>();
            string query = "SELECT CategoryID, CategoryName FROM CategoryMap";

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
                                categories.Add(new Category
                                {
                                    CategoryID = reader["CategoryID"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                });
                            }
                        }
                    }
                }

                cmbCategory.ItemsSource = categories;
                cmbCategory.SelectedValuePath = "CategoryID"; // Keep this to retrieve the CategoryID as the selected value
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadInventoryItems(string categoryId)
        {
            catCountItems = new ObservableCollection<CountItem>();
            string locationID = Properties.Settings.Default.LocationID;

            string query = @"
    SELECT i.SKU, c.ProductName, i.QuantityOnHandSellable 
    FROM Inventory i
    INNER JOIN Catalog c ON i.SKU = c.SKU
    WHERE i.CategoryID = @CategoryID AND i.LocationID = @LocationID AND i.QuantityOnHandSellable != 0";

            try
            {
                using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryID", categoryId);
                        command.Parameters.AddWithValue("@LocationID", locationID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string sku = reader["SKU"].ToString();

                                // Skip BaseSKUs
                                if (IsBaseSKU(sku))
                                {
                                    Console.WriteLine($"SKU {sku} is a BaseSKU and will be omitted.");
                                    continue;
                                }

                                string productName = reader["ProductName"].ToString();
                                int quantityOnHand = Convert.ToInt32(reader["QuantityOnHandSellable"]);

                                catCountItems.Add(new CountItem
                                {
                                    SKU = sku,
                                    ProductName = productName,
                                    ExpectedQuantity = quantityOnHand,
                                    ScannedQuantity = 0
                                });
                            }
                        }
                    }
                }

                lvInventoryItems.ItemsSource = catCountItems;

                // Log total items loaded
                Console.WriteLine($"Total items loaded: {catCountItems.Count}");
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading inventory items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void StartNewCount_Click(object sender, RoutedEventArgs e)
        {
            if (isCountInProgress)
            {
                MessageBox.Show("A count is already in progress. Please finalize the current count before starting a new one.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbCategory.SelectedValue != null)
            {
                var result = MessageBox.Show("Do you want to start a new count for this category?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    string selectedCategoryId = cmbCategory.SelectedValue.ToString();
                    LoadInventoryItems(selectedCategoryId);

                    lvInventoryItems.IsEnabled = true;
                    InputPanel.IsEnabled = true;
                    FinalizeCountButton.IsEnabled = true;

                    isCountInProgress = true;
                }
            }
            else
            {
                MessageBox.Show("Please select a category before starting the count.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddCountButton_Click(object sender, RoutedEventArgs e)
        {
            string inputSKU = txtSKU.Text;
            if (string.IsNullOrWhiteSpace(inputSKU))
            {
                MessageBox.Show("Please enter a SKU.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantityToAdd))
            {
                MessageBox.Show("Invalid quantity. Please enter a valid number.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CountItem itemToUpdate = catCountItems.FirstOrDefault(item => item.SKU == inputSKU);
            if (itemToUpdate != null)
            {
                itemToUpdate.ScannedQuantity += quantityToAdd;
                lvInventoryItems.Items.Refresh();
            }
            else
            {
                MessageBox.Show($"SKU {inputSKU} not found in the selected category.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            txtSKU.Clear();
            txtQuantity.Text = "1";
        }

        private void FinalizeCountButton_Click(object sender, RoutedEventArgs e)
        {
            if (catCountItems.Count == 0)
            {
                MessageBox.Show("No items to finalize.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to finalize the count?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmResult == MessageBoxResult.Yes)
            {
                string countID = Guid.NewGuid().ToString();
                string locationID = Properties.Settings.Default.LocationID;
                string employeeID = "488447"; // Replace with the logged-in employee ID
                DateTime completionDate = DateTime.Now;

                string insertCountQuery = @"
            INSERT INTO Counts 
            (CountID, LocationID, CountType, CountCategory, CountCompletionDate, CountEmployeeIDCompleted) 
            VALUES 
            (@CountID, @LocationID, 'CAT', @CategoryID, @CompletionDate, @EmployeeID)";

                string insertDetailsQuery = @"
            INSERT INTO CountDetails 
            (CountID, LocationID, SKU, SKUQuantityExpected, SKUQuantityActual, SKUDiscrepancy) 
            VALUES 
            (@CountID, @LocationID, @SKU, @SKUQuantityExpected, @SKUQuantityActual, @SKUDiscrepancy)";

                string updateCountsQuery = @"
            UPDATE Counts
            SET 
                CountQuantityExpectedTotal = (SELECT SUM(SKUQuantityExpected) FROM CountDetails WHERE CountID = @CountID),
                CountQuantityActualTotal = (SELECT SUM(SKUQuantityActual) FROM CountDetails WHERE CountID = @CountID),
                CountTotalDiscrepancies = (SELECT SUM(ABS(SKUDiscrepancy)) FROM CountDetails WHERE CountID = @CountID),
                CountAccuracy = 
                    CASE 
                        WHEN (SELECT SUM(SKUQuantityExpected) FROM CountDetails WHERE CountID = @CountID) = 0 THEN 0
                        ELSE (CAST((SELECT SUM(SKUQuantityActual) FROM CountDetails WHERE CountID = @CountID) AS DECIMAL(12,2)) /
                              CAST((SELECT SUM(SKUQuantityExpected) FROM CountDetails WHERE CountID = @CountID) AS DECIMAL(12,2)) * 100)
                    END
            WHERE CountID = @CountID";

                string updateInventoryQuery = @"
            UPDATE Inventory
            SET QuantityOnHandSellable = @SKUQuantityActual
            WHERE SKU = @SKU AND LocationID = @LocationID";

                string insertAdjustmentQuery = @"
            INSERT INTO Adjustments 
            (AdjustmentID, LocationID, SKU, AdjustmentDate, AdjustmentTime, AdjustmentEmployeeID, Quantity, AdjustmentType) 
            VALUES 
            (@AdjustmentID, @LocationID, @SKU, @AdjustmentDate, @AdjustmentTime, @AdjustmentEmployeeID, @Quantity, 'Inventory Count')";

                try
                {
                    using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                    {
                        connection.Open();

                        // Insert into Counts table
                        using (SqlCommand countCommand = new SqlCommand(insertCountQuery, connection))
                        {
                            countCommand.Parameters.AddWithValue("@CountID", countID);
                            countCommand.Parameters.AddWithValue("@LocationID", locationID);
                            countCommand.Parameters.AddWithValue("@CategoryID", cmbCategory.SelectedValue.ToString());
                            countCommand.Parameters.AddWithValue("@CompletionDate", completionDate);
                            countCommand.Parameters.AddWithValue("@EmployeeID", employeeID);

                            countCommand.ExecuteNonQuery();
                        }

                        // Insert into CountDetails table and update inventory
                        foreach (var item in catCountItems)
                        {
                            if (IsBaseSKU(item.SKU))
                            {
                                Console.WriteLine($"SKU {item.SKU} is a BaseSKU and will be omitted from finalizing.");
                                continue; // Skip BaseSKUs
                            }

                            int discrepancy = item.ScannedQuantity - item.ExpectedQuantity;

                            using (SqlCommand detailCommand = new SqlCommand(insertDetailsQuery, connection))
                            {
                                detailCommand.Parameters.AddWithValue("@CountID", countID);
                                detailCommand.Parameters.AddWithValue("@LocationID", locationID);
                                detailCommand.Parameters.AddWithValue("@SKU", item.SKU);
                                detailCommand.Parameters.AddWithValue("@SKUQuantityExpected", item.ExpectedQuantity);
                                detailCommand.Parameters.AddWithValue("@SKUQuantityActual", item.ScannedQuantity);
                                detailCommand.Parameters.AddWithValue("@SKUDiscrepancy", discrepancy);

                                detailCommand.ExecuteNonQuery();
                            }

                            using (SqlCommand inventoryCommand = new SqlCommand(updateInventoryQuery, connection))
                            {
                                inventoryCommand.Parameters.AddWithValue("@SKU", item.SKU);
                                inventoryCommand.Parameters.AddWithValue("@LocationID", locationID);
                                inventoryCommand.Parameters.AddWithValue("@SKUQuantityActual", item.ScannedQuantity);

                                inventoryCommand.ExecuteNonQuery();
                            }

                            using (SqlCommand adjustmentCommand = new SqlCommand(insertAdjustmentQuery, connection))
                            {
                                adjustmentCommand.Parameters.AddWithValue("@AdjustmentID", Guid.NewGuid().ToString());
                                adjustmentCommand.Parameters.AddWithValue("@LocationID", locationID);
                                adjustmentCommand.Parameters.AddWithValue("@SKU", item.SKU);
                                adjustmentCommand.Parameters.AddWithValue("@AdjustmentDate", completionDate.Date);
                                adjustmentCommand.Parameters.AddWithValue("@AdjustmentTime", completionDate.TimeOfDay);
                                adjustmentCommand.Parameters.AddWithValue("@AdjustmentEmployeeID", employeeID);
                                adjustmentCommand.Parameters.AddWithValue("@Quantity", discrepancy);

                                adjustmentCommand.ExecuteNonQuery();
                            }
                        }

                        // Update the Counts table with aggregated values
                        using (SqlCommand updateCommand = new SqlCommand(updateCountsQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@CountID", countID);
                            updateCommand.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Count finalized successfully, and inventory adjusted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    isCountInProgress = false;
                    lvInventoryItems.IsEnabled = false;
                    InputPanel.IsEnabled = false;
                    FinalizeCountButton.IsEnabled = false;
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error finalizing count: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }




        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedItem is Category selectedCategory)
            {
                txtCurrentCategory.Text = $"Current Category: {selectedCategory.CategoryID} - {selectedCategory.CategoryName}";
            }
        }

        private void LvInventoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvInventoryItems.SelectedItem is CountItem selectedItem)
            {
                txtSKU.Text = selectedItem.SKU;
            }
        }

        private void CountHistory_Click(object sender, RoutedEventArgs e)
        {
            CountHistoryWindow countHistoryWindow = new CountHistoryWindow();
            countHistoryWindow.ShowDialog();
        }

        private bool IsBaseSKU(string sku)
        {
            bool isBaseSKU = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
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

    }
}
