using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.CatalogManagerPages
{
    public partial class CatalogSearchPage : Page
    {
        public DatabaseHelper dbHelper = new DatabaseHelper();

        public CatalogSearchPage()
        {
            InitializeComponent();
            LoadCategories();
            // LoadAllProducts(); // Removed initial product load
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
                            List<Category> categories = new List<Category>();
                            categories.Add(new Category { CategoryID = "", CategoryName = "All Categories" });

                            while (reader.Read())
                            {
                                categories.Add(new Category
                                {
                                    CategoryID = reader["CategoryID"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                });
                            }

                            CategoryComboBox.ItemsSource = categories;
                            CategoryComboBox.SelectedIndex = 0; // Default to "All Categories"
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
                                   "WHERE 1 = 1"; // 1=1 ensures additional AND conditions can be appended without breaking

                    // Apply filters
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
                        // Add parameters
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

                        // Execute the query
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
                                    Price = Convert.ToDecimal(reader["Price"])
                                });
                            }
                        }

                        // Bind the retrieved products to the DataGrid
                        ProductDataGrid.ItemsSource = products;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Product class to hold catalog items
        public class Product
        {
            public string SKU { get; set; }
            public string ProductName { get; set; }
            public string CategoryName { get; set; }
            public decimal Price { get; set; }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string sku = SkuTextBox.Text.Trim();
            string productName = ProductNameTextBox.Text.Trim();
            string categoryId = (CategoryComboBox.SelectedValue ?? "").ToString();
            decimal? minPrice = null, maxPrice = null;

            // Try parsing the price values
            if (decimal.TryParse(MinPriceTextBox.Text, out decimal parsedMinPrice))
                minPrice = parsedMinPrice;
            if (decimal.TryParse(MaxPriceTextBox.Text, out decimal parsedMaxPrice))
                maxPrice = parsedMaxPrice;

            // Check if the category is selected and no other filters are provided
            if (!string.IsNullOrWhiteSpace(categoryId) && categoryId != "All Categories" &&
                string.IsNullOrWhiteSpace(sku) && string.IsNullOrWhiteSpace(productName) &&
                !minPrice.HasValue && !maxPrice.HasValue)
            {
                // Load all products within the selected category
                LoadProducts(null, null, categoryId, null);
            }
            else
            {
                // Apply other filters, if any
                LoadProducts(sku, productName, categoryId, (minPrice, maxPrice));
            }
        }

        // Reset button click event to clear filters
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SkuTextBox.Clear();
            ProductNameTextBox.Clear();
            CategoryComboBox.SelectedIndex = 0;
            MinPriceTextBox.Clear();
            MaxPriceTextBox.Clear();

            ProductDataGrid.ItemsSource = null; // Clear the grid
        }
    }
}
