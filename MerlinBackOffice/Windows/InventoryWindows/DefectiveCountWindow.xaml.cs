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
    public partial class DefectiveCountWindow : Window
    {
        private ObservableCollection<CountItem> defectiveItems;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();
        private bool isCountInProgress = false;

        public DefectiveCountWindow()
        {
            InitializeComponent();
            LoadDefectiveItems();
        }

        private void LoadDefectiveItems()
        {
            defectiveItems = new ObservableCollection<CountItem>();
            string locationID = Properties.Settings.Default.LocationID;

            string query = @"
        SELECT i.SKU, c.ProductName, i.CategoryID, i.QuantityOnHandDefective 
        FROM Inventory i
        INNER JOIN Catalog c ON i.SKU = c.SKU
        WHERE i.LocationID = @LocationID AND i.QuantityOnHandDefective > 0";

            try
            {
                using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
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

                                defectiveItems.Add(new CountItem
                                {
                                    SKU = sku,
                                    ProductName = reader["ProductName"].ToString(),
                                    CategoryID = reader["CategoryID"].ToString(),
                                    ExpectedQuantity = Convert.ToInt32(reader["QuantityOnHandDefective"]),
                                    ScannedQuantity = 0
                                });
                            }
                        }
                    }
                }

                lvDefectiveItems.ItemsSource = defectiveItems;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading defective items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void StartNewCount_Click(object sender, RoutedEventArgs e)
        {
            if (isCountInProgress)
            {
                MessageBox.Show("A count is already in progress. Please finalize the current count before starting a new one.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lvDefectiveItems.IsEnabled = true;
            InputPanel.IsEnabled = true;
            FinalizeCountButton.IsEnabled = true;

            isCountInProgress = true;
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

            CountItem itemToUpdate = defectiveItems.FirstOrDefault(item => item.SKU == inputSKU);
            if (itemToUpdate != null)
            {
                itemToUpdate.ScannedQuantity += quantityToAdd;
                lvDefectiveItems.Items.Refresh();
            }
            else
            {
                MessageBox.Show($"SKU {inputSKU} not found in defective items.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            txtSKU.Clear();
            txtQuantity.Text = "1";
        }

        private void FinalizeCountButton_Click(object sender, RoutedEventArgs e)
        {
            if (defectiveItems.Count == 0)
            {
                MessageBox.Show("No items to finalize.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to finalize the defective count?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmResult == MessageBoxResult.Yes)
            {
                string countID = Guid.NewGuid().ToString();
                string locationID = Properties.Settings.Default.LocationID;
                string employeeID = "488447"; // Replace with logged-in employee ID
                DateTime completionDate = DateTime.Now;

                string insertDefectiveCountQuery = @"
                INSERT INTO Counts 
                (CountID, LocationID, CountType, CountCompletionDate, CountEmployeeIDCompleted) 
                VALUES 
                (@CountID, @LocationID, 'DEF', @CompletionDate, @EmployeeID)";

                string insertDefectiveDetailsQuery = @"
                INSERT INTO CountDetails 
                (CountID, LocationID, SKU, SKUQuantityExpected, SKUQuantityActual, SKUDiscrepancy) 
                VALUES 
                (@CountID, @LocationID, @SKU, @SKUQuantityExpected, @SKUQuantityActual, @SKUDiscrepancy)";

                string updateInventoryQuery = @"
                UPDATE Inventory
                SET QuantityOnHandDefective = @SKUQuantityActual
                WHERE SKU = @SKU AND LocationID = @LocationID";

                string insertAdjustmentQuery = @"
                INSERT INTO Adjustments 
                (AdjustmentID, LocationID, SKU, AdjustmentDate, AdjustmentTime, AdjustmentEmployeeID, Quantity, AdjustmentType) 
                VALUES 
                (@AdjustmentID, @LocationID, @SKU, @AdjustmentDate, @AdjustmentTime, @AdjustmentEmployeeID, @Quantity, 'Defective Count')";

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

                try
                {
                    using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                    {
                        connection.Open();

                        using (SqlCommand countCommand = new SqlCommand(insertDefectiveCountQuery, connection))
                        {
                            countCommand.Parameters.AddWithValue("@CountID", countID);
                            countCommand.Parameters.AddWithValue("@LocationID", locationID);
                            countCommand.Parameters.AddWithValue("@CompletionDate", completionDate);
                            countCommand.Parameters.AddWithValue("@EmployeeID", employeeID);

                            countCommand.ExecuteNonQuery();
                        }

                        foreach (var item in defectiveItems)
                        {
                            // Skip BaseSKUs
                            if (IsBaseSKU(item.SKU))
                            {
                                Console.WriteLine($"SKU {item.SKU} is a BaseSKU and will be omitted from finalizing.");
                                continue;
                            }

                            int discrepancy = item.ScannedQuantity - item.ExpectedQuantity;

                            using (SqlCommand detailCommand = new SqlCommand(insertDefectiveDetailsQuery, connection))
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

                        using (SqlCommand updateCommand = new SqlCommand(updateCountsQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@CountID", countID);
                            updateCommand.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Defective count finalized successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    isCountInProgress = false;
                    lvDefectiveItems.IsEnabled = false;
                    InputPanel.IsEnabled = false;
                    FinalizeCountButton.IsEnabled = false;
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error finalizing defective count: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void lvDefectiveItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvDefectiveItems.SelectedItem is CountItem selectedItem)
            {
                txtSKU.Text = selectedItem.SKU;
            }
        }

        private void CountHistory_click(object sender, RoutedEventArgs e)
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
