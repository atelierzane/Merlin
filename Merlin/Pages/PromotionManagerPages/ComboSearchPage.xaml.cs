using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class ComboSearchPage : Page
    {
        public DatabaseHelper dbHelper = new DatabaseHelper();

        public ComboSearchPage()
        {
            InitializeComponent();
        }

        // Load combos based on search criteria
        private void LoadCombos(string comboSKU, string comboName, (decimal? minPrice, decimal? maxPrice)? priceRange)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ComboSKU, ComboName, ComboPrice FROM Combos WHERE 1=1";

                    // Apply filters
                    if (!string.IsNullOrWhiteSpace(comboSKU))
                    {
                        query += " AND ComboSKU = @ComboSKU";
                    }
                    if (!string.IsNullOrWhiteSpace(comboName))
                    {
                        query += " AND ComboName LIKE @ComboName";
                    }
                    if (priceRange.HasValue)
                    {
                        if (priceRange.Value.minPrice.HasValue)
                        {
                            query += " AND ComboPrice >= @MinPrice";
                        }
                        if (priceRange.Value.maxPrice.HasValue)
                        {
                            query += " AND ComboPrice <= @MaxPrice";
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters
                        if (!string.IsNullOrWhiteSpace(comboSKU))
                            cmd.Parameters.AddWithValue("@ComboSKU", comboSKU);
                        if (!string.IsNullOrWhiteSpace(comboName))
                            cmd.Parameters.AddWithValue("@ComboName", $"%{comboName}%");
                        if (priceRange.HasValue)
                        {
                            if (priceRange.Value.minPrice.HasValue)
                                cmd.Parameters.AddWithValue("@MinPrice", priceRange.Value.minPrice);
                            if (priceRange.Value.maxPrice.HasValue)
                                cmd.Parameters.AddWithValue("@MaxPrice", priceRange.Value.maxPrice);
                        }

                        // Execute the query
                        List<Combo> combos = new List<Combo>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                combos.Add(new Combo
                                {
                                    ComboSKU = reader["ComboSKU"].ToString(),
                                    ComboName = reader["ComboName"].ToString(),
                                    ComboPrice = Convert.ToDecimal(reader["ComboPrice"])
                                });
                            }
                        }

                        // Bind the retrieved combos to the DataGrid
                        ComboDataGrid.ItemsSource = combos;
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
            string comboSKU = ComboSkuTextBox.Text.Trim();
            string comboName = ComboNameTextBox.Text.Trim();
            decimal? minPrice = null, maxPrice = null;

            // Try parsing the price values
            if (decimal.TryParse(MinPriceTextBox.Text, out decimal parsedMinPrice))
                minPrice = parsedMinPrice;
            if (decimal.TryParse(MaxPriceTextBox.Text, out decimal parsedMaxPrice))
                maxPrice = parsedMaxPrice;

            // Apply filters, if any
            LoadCombos(comboSKU, comboName, (minPrice, maxPrice));
        }

        // Reset button click event to clear filters
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ComboSkuTextBox.Clear();
            ComboNameTextBox.Clear();
            MinPriceTextBox.Clear();
            MaxPriceTextBox.Clear();

            ComboDataGrid.ItemsSource = null; // Clear the grid
        }
    }
}
