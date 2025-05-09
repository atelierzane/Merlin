using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.CatalogManagerPages
{
    public partial class EditProductBulkPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EditProductBulkPage()
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

        // Search button click event
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

        // Reset button click event
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SkuTextBox.Clear();
            ProductNameTextBox.Clear();
            CategoryComboBox.SelectedIndex = 0;
            MinPriceTextBox.Clear();
            MaxPriceTextBox.Clear();
            ProductDataGrid.ItemsSource = null; // Clear the grid
        }

        // Save changes for selected products
        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProducts = new List<Product>();

            foreach (Product product in ProductDataGrid.Items)
            {
                if (product.IsSelected)
                {
                    selectedProducts.Add(product);
                }
            }

            if (selectedProducts.Count == 0)
            {
                MessageBox.Show("Please select at least one product to update.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to save changes for {selectedProducts.Count} product(s)?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        foreach (var product in selectedProducts)
                        {
                            string updateQuery = "UPDATE Catalog SET ProductName = @ProductName, Price = @Price WHERE SKU = @SKU";
                            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                                cmd.Parameters.AddWithValue("@Price", product.Price);
                                cmd.Parameters.AddWithValue("@SKU", product.SKU);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedProducts.Count} product(s) updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        SearchButton_Click(sender, e); // Refresh the data grid after updates
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
