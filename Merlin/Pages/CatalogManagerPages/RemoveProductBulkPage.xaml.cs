using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.CatalogManagerPages
{
    public partial class RemoveProductBulkPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public RemoveProductBulkPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        // Load categories into the ComboBox
        private void LoadCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName FROM CategoryMap";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<Category> categories = new List<Category>
                            {
                                new Category { CategoryID = "", CategoryName = "All Categories" }
                            };

                            while (reader.Read())
                            {
                                categories.Add(new Category
                                {
                                    CategoryID = reader["CategoryID"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                });
                            }

                            CategoryComboBox.ItemsSource = categories;
                            CategoryComboBox.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load products based on search criteria
        private void LoadProducts(string sku, string productName, string categoryId, (decimal? minPrice, decimal? maxPrice)? priceRange)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT SKU, ProductName, CategoryName, Price " +
                                   "FROM Catalog C " +
                                   "JOIN CategoryMap CM ON C.CategoryID = CM.CategoryID " +
                                   "WHERE 1=1"; // Allows for additional filtering conditions

                    if (!string.IsNullOrWhiteSpace(sku))
                    {
                        query += " AND C.SKU = @SKU";
                    }
                    if (!string.IsNullOrWhiteSpace(productName))
                    {
                        query += " AND C.ProductName LIKE @ProductName";
                    }
                    if (!string.IsNullOrWhiteSpace(categoryId) && categoryId != "All Categories")
                    {
                        query += " AND C.CategoryID = @CategoryID";
                    }
                    if (priceRange.HasValue)
                    {
                        if (priceRange.Value.minPrice.HasValue)
                        {
                            query += " AND C.Price >= @MinPrice";
                        }
                        if (priceRange.Value.maxPrice.HasValue)
                        {
                            query += " AND C.Price <= @MaxPrice";
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(sku))
                            cmd.Parameters.AddWithValue("@SKU", sku);
                        if (!string.IsNullOrWhiteSpace(productName))
                            cmd.Parameters.AddWithValue("@ProductName", $"%{productName}%");
                        if (!string.IsNullOrWhiteSpace(categoryId) && categoryId != "All Categories")
                            cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                        if (priceRange.HasValue)
                        {
                            if (priceRange.Value.minPrice.HasValue)
                                cmd.Parameters.AddWithValue("@MinPrice", priceRange.Value.minPrice);
                            if (priceRange.Value.maxPrice.HasValue)
                                cmd.Parameters.AddWithValue("@MaxPrice", priceRange.Value.maxPrice);
                        }

                        List<Product> products = new List<Product>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    SKU = reader["SKU"].ToString(),
                                    ProductName = reader["ProductName"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    IsSelected = false // Initially not selected
                                });
                            }
                        }

                        ProductDataGrid.ItemsSource = products;
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
            string sku = SkuTextBox.Text.Trim();
            string productName = ProductNameTextBox.Text.Trim();
            string categoryId = (CategoryComboBox.SelectedValue ?? "").ToString();
            decimal? minPrice = null, maxPrice = null;

            if (decimal.TryParse(MinPriceTextBox.Text, out decimal parsedMinPrice))
                minPrice = parsedMinPrice;
            if (decimal.TryParse(MaxPriceTextBox.Text, out decimal parsedMaxPrice))
                maxPrice = parsedMaxPrice;

            LoadProducts(sku, productName, categoryId, (minPrice, maxPrice));
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SkuTextBox.Clear();
            ProductNameTextBox.Clear();
            CategoryComboBox.SelectedIndex = 0;
            MinPriceTextBox.Clear();
            MaxPriceTextBox.Clear();
            ProductDataGrid.ItemsSource = null; // Clear the grid
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (sender as CheckBox).IsChecked == true;

            // Iterate through the items in the DataGrid
            foreach (var item in ProductDataGrid.Items)
            {
                // Ensure the item is of type Product before casting
                if (item is Product product)
                {
                    product.IsSelected = isChecked;
                }
            }

            // Refresh the DataGrid to reflect the changes
            ProductDataGrid.Items.Refresh();
        }


        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProducts = new List<Product>();

            // Iterate through the items in the DataGrid and filter only Product objects
            foreach (var item in ProductDataGrid.Items)
            {
                if (item is Product product && product.IsSelected)
                {
                    selectedProducts.Add(product);
                }
            }

            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Please select at least one product to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {selectedProducts.Count} product(s)?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        // Start a transaction to ensure atomicity of both catalog and inventory deletions
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                foreach (var product in selectedProducts)
                                {
                                    // Delete product from the Catalog
                                    string deleteCatalogQuery = "DELETE FROM Catalog WHERE SKU = @SKU";
                                    using (SqlCommand deleteCatalogCmd = new SqlCommand(deleteCatalogQuery, conn, transaction))
                                    {
                                        deleteCatalogCmd.Parameters.AddWithValue("@SKU", product.SKU);
                                        deleteCatalogCmd.ExecuteNonQuery();
                                    }

                                    // Delete product from the Inventory for all locations
                                    string deleteInventoryQuery = "DELETE FROM Inventory WHERE SKU = @SKU";
                                    using (SqlCommand deleteInventoryCmd = new SqlCommand(deleteInventoryQuery, conn, transaction))
                                    {
                                        deleteInventoryCmd.Parameters.AddWithValue("@SKU", product.SKU);
                                        deleteInventoryCmd.ExecuteNonQuery();
                                    }
                                }

                                // Commit the transaction if all deletions were successful
                                transaction.Commit();
                                MessageBox.Show($"{selectedProducts.Count} product(s) deleted successfully from catalog and inventory.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Refresh the data grid after deletion
                                SearchButton_Click(sender, e);
                            }
                            catch (Exception ex)
                            {
                                // Rollback the transaction if there was an error
                                transaction.Rollback();
                                MessageBox.Show($"Error removing products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
