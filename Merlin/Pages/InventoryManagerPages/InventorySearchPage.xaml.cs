using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.InventoryManagerPages
{
    public partial class InventorySearchPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming you have a DatabaseHelper class

        public InventorySearchPage()
        {
            InitializeComponent();
        }

        // Event handler for the Search button
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string sku = txtSKU.Text.Trim();
            string productName = txtProductName.Text.Trim();
            string location = txtLocation.Text.Trim();
            string category = txtCategory.Text.Trim();

            // Perform search and load data into the DataGrid
            LoadInventoryResults(sku, productName, location, category);
        }

        // Event handler for the Reset button
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear search fields
            txtSKU.Clear();
            txtProductName.Clear();
            txtLocation.Clear();
            txtCategory.Clear();
            dgInventoryResults.ItemsSource = null; // Clear the DataGrid
        }

        // Method to load inventory results into the DataGrid
        private void LoadInventoryResults(string sku, string productName, string location, string category)
        {
            List<InventoryItem> inventoryItems = new List<InventoryItem>();

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT i.SKU, c.ProductName, i.LocationID, i.CategoryID, i.QuantityOnHandSellable, i.QuantityOnHandDefective 
                                     FROM Inventory i
                                     INNER JOIN Catalog c ON i.SKU = c.SKU
                                     WHERE 1=1";

                    if (!string.IsNullOrEmpty(sku))
                        query += " AND i.SKU LIKE @SKU";
                    if (!string.IsNullOrEmpty(productName))
                        query += " AND c.ProductName LIKE @ProductName";
                    if (!string.IsNullOrEmpty(location))
                        query += " AND i.LocationID LIKE @Location";
                    if (!string.IsNullOrEmpty(category))
                        query += " AND i.CategoryID LIKE @Category";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(sku))
                            cmd.Parameters.AddWithValue("@SKU", "%" + sku + "%");
                        if (!string.IsNullOrEmpty(productName))
                            cmd.Parameters.AddWithValue("@ProductName", "%" + productName + "%");
                        if (!string.IsNullOrEmpty(location))
                            cmd.Parameters.AddWithValue("@Location", "%" + location + "%");
                        if (!string.IsNullOrEmpty(category))
                            cmd.Parameters.AddWithValue("@Category", "%" + category + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                inventoryItems.Add(new InventoryItem
                                {
                                    SKU = reader["SKU"].ToString(),
                                    ProductName = reader["ProductName"].ToString(),
                                    LocationID = reader["LocationID"].ToString(),
                                    CategoryID = reader["CategoryID"].ToString(),
                                    QuantityOnHandSellable = Convert.ToInt32(reader["QuantityOnHandSellable"]),
                                    QuantityOnHandDefective = Convert.ToInt32(reader["QuantityOnHandDefective"])
                                });
                            }
                        }
                    }
                }

                dgInventoryResults.ItemsSource = inventoryItems; // Bind results to the DataGrid
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshInventory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Get all SKUs and their associated CategoryID from the Catalog
                    string getSkusQuery = "SELECT SKU, CategoryID FROM Catalog";
                    Dictionary<string, string> skuCategoryMap = new Dictionary<string, string>();
                    using (SqlCommand cmd = new SqlCommand(getSkusQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string sku = reader["SKU"].ToString();
                                string categoryID = reader["CategoryID"].ToString();
                                skuCategoryMap.Add(sku, categoryID);
                            }
                        }
                    }

                    // Get all locations
                    string getLocationsQuery = "SELECT LocationID FROM Location";
                    List<string> allLocations = new List<string>();
                    using (SqlCommand cmd = new SqlCommand(getLocationsQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allLocations.Add(reader["LocationID"].ToString());
                            }
                        }
                    }

                    // Check for missing SKUs in each location's inventory
                    foreach (string locationID in allLocations)
                    {
                        foreach (KeyValuePair<string, string> skuCategoryPair in skuCategoryMap)
                        {
                            string sku = skuCategoryPair.Key;
                            string categoryID = skuCategoryPair.Value;

                            // Check if the SKU exists in the inventory for this location
                            string checkQuery = "SELECT COUNT(*) FROM Inventory WHERE SKU = @SKU AND LocationID = @LocationID";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@SKU", sku);
                                checkCmd.Parameters.AddWithValue("@LocationID", locationID);
                                int count = (int)checkCmd.ExecuteScalar();

                                if (count == 0)
                                {
                                    // Insert missing SKU with zero quantities and the correct CategoryID
                                    string insertQuery = "INSERT INTO Inventory (SKU, LocationID, CategoryID, QuantityOnHandSellable, QuantityOnHandDefective) " +
                                                         "VALUES (@SKU, @LocationID, @CategoryID, 0, 0)";
                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@SKU", sku);
                                        insertCmd.Parameters.AddWithValue("@LocationID", locationID);
                                        insertCmd.Parameters.AddWithValue("@CategoryID", categoryID);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    MessageBox.Show("Inventory refresh completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error refreshing inventory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAllInventory_Click(object sender, RoutedEventArgs e)
        {
            // Confirm deletion
            var result = MessageBox.Show(
                "This will delete all inventory items from all locations. Are you sure you want to proceed?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();

                        // Delete all items from the Inventory table
                        string deleteQuery = "DELETE FROM Inventory";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("All inventory items have been deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error deleting inventory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        // Class representing the data model for the inventory

    }
}
