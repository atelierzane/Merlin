using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class RemoveComboBulkPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private ObservableCollection<Combo> comboCollection; // Use ObservableCollection to bind the DataGrid

        public RemoveComboBulkPage()
        {
            InitializeComponent();
            comboCollection = new ObservableCollection<Combo>(); // Initialize the ObservableCollection
            ComboDataGrid.ItemsSource = comboCollection;         // Bind the DataGrid to the ObservableCollection
        }

        // Load combos based on search criteria
        private void LoadCombos(string comboSKU, string comboName, (decimal? minPrice, decimal? maxPrice)? priceRange)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ComboSKU, ComboName, ComboPrice FROM Combos WHERE 1=1"; // Allows for additional filtering conditions

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

                        comboCollection.Clear(); // Clear the previous items

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                comboCollection.Add(new Combo
                                {
                                    ComboSKU = reader["ComboSKU"].ToString(),
                                    ComboName = reader["ComboName"].ToString(),
                                    ComboPrice = Convert.ToDecimal(reader["ComboPrice"]),
                                    IsSelected = false // Initially not selected
                                });
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

        // Search combos based on the filters
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string comboSKU = SkuTextBox.Text.Trim();
            string comboName = ComboNameTextBox.Text.Trim();
            decimal? minPrice = null, maxPrice = null;

            if (decimal.TryParse(MinPriceTextBox.Text, out decimal parsedMinPrice))
                minPrice = parsedMinPrice;
            if (decimal.TryParse(MaxPriceTextBox.Text, out decimal parsedMaxPrice))
                maxPrice = parsedMaxPrice;

            LoadCombos(comboSKU, comboName, (minPrice, maxPrice));
        }

        // Reset search criteria
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SkuTextBox.Clear();
            ComboNameTextBox.Clear();
            MinPriceTextBox.Clear();
            MaxPriceTextBox.Clear();
            comboCollection.Clear(); // Clear the collection to reset the grid
        }

        // Select or deselect all items in the DataGrid
        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (sender as CheckBox).IsChecked == true;

            foreach (var combo in comboCollection)
            {
                combo.IsSelected = isChecked; // Update the IsSelected property for each combo
            }

            ComboDataGrid.Items.Refresh(); // Refresh the DataGrid to reflect the changes
        }

        // Delete selected combos
        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCombos = new List<Combo>();

            // Filter only the selected combos
            foreach (var combo in comboCollection)
            {
                if (combo.IsSelected)
                {
                    selectedCombos.Add(combo);
                }
            }

            if (selectedCombos.Count == 0)
            {
                MessageBox.Show("Please select at least one combo to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {selectedCombos.Count} combo(s)?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                foreach (var combo in selectedCombos)
                                {
                                    // Delete combo from the Combos table
                                    string deleteComboQuery = "DELETE FROM Combos WHERE ComboSKU = @ComboSKU";
                                    using (SqlCommand deleteComboCmd = new SqlCommand(deleteComboQuery, conn, transaction))
                                    {
                                        deleteComboCmd.Parameters.AddWithValue("@ComboSKU", combo.ComboSKU);
                                        deleteComboCmd.ExecuteNonQuery();
                                    }

                                    // Delete combo items from the ComboItems table
                                    string deleteComboItemsQuery = "DELETE FROM ComboItems WHERE ComboSKU = @ComboSKU";
                                    using (SqlCommand deleteComboItemsCmd = new SqlCommand(deleteComboItemsQuery, conn, transaction))
                                    {
                                        deleteComboItemsCmd.Parameters.AddWithValue("@ComboSKU", combo.ComboSKU);
                                        deleteComboItemsCmd.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                                MessageBox.Show($"{selectedCombos.Count} combo(s) deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Refresh the data grid after deletion
                                SearchButton_Click(sender, e);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Error removing combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
