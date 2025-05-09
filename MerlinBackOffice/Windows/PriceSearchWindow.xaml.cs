using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using MerlinBackOffice.Helpers;
using MerlinBackOffice.Models;

namespace MerlinBackOffice.Windows
{
    public partial class PriceSearchWindow : Window
    {
        private ObservableCollection<SearchResultItem> searchResults;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();

        public PriceSearchWindow()
        {
            InitializeComponent();
        }

        private void OnSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string sku = SkuTextBox.Text.Trim();
            string category = CategoryTextBox.Text.Trim();
            string minPrice = MinPriceTextBox.Text.Trim();
            string maxPrice = MaxPriceTextBox.Text.Trim();
            string minQuantity = MinQuantityTextBox.Text.Trim();
            string maxQuantity = MaxQuantityTextBox.Text.Trim();
            string locationID = Properties.Settings.Default.LocationID;

            searchResults = new ObservableCollection<SearchResultItem>();

            try
            {
                using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    string query;

                    if (!string.IsNullOrEmpty(sku))
                    {
                        // Check if the entered SKU is a BaseSKU
                        string baseSkuCheckQuery = "SELECT IsBaseSKU FROM Catalog WHERE SKU = @SKU";

                        using (SqlCommand checkCmd = new SqlCommand(baseSkuCheckQuery, connection))
                        {
                            checkCmd.Parameters.AddWithValue("@SKU", sku);
                            var isBaseSKU = checkCmd.ExecuteScalar();

                            if (isBaseSKU != null && Convert.ToBoolean(isBaseSKU))
                            {
                                // Fetch all variants of the BaseSKU
                                query = @"
                        SELECT 
                            i.CategoryID,
                            i.SKU,
                            c.ProductName AS Title,
                            c.Price,
                            i.QuantityOnHandSellable AS SellableQuantity,
                            i.QuantityOnHandDefective AS DefectiveQuantity,
                            i.QuantityOnHandSellable AS ExpectedSellableQuantity,
                            i.QuantityOnHandDefective AS ExpectedDefectiveQuantity
                        FROM Inventory i
                        INNER JOIN Catalog c ON i.SKU = c.SKU
                        WHERE c.VariantAssignedToBaseSKU = @BaseSKU AND i.LocationID = @LocationID";

                                using (SqlCommand variantCmd = new SqlCommand(query, connection))
                                {
                                    variantCmd.Parameters.AddWithValue("@BaseSKU", sku);
                                    variantCmd.Parameters.AddWithValue("@LocationID", locationID);

                                    using (SqlDataReader reader = variantCmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            searchResults.Add(new SearchResultItem
                                            {
                                                CategoryID = reader["CategoryID"].ToString(),
                                                SKU = reader["SKU"].ToString(),
                                                Title = reader["Title"].ToString(),
                                                Price = Convert.ToDecimal(reader["Price"]),
                                                SellableQuantity = Convert.ToInt32(reader["SellableQuantity"]),
                                                DefectiveQuantity = Convert.ToInt32(reader["DefectiveQuantity"]),
                                                ExpectedSellableQuantity = Convert.ToInt32(reader["ExpectedSellableQuantity"]),
                                                ExpectedDefectiveQuantity = Convert.ToInt32(reader["ExpectedDefectiveQuantity"])
                                            });
                                        }
                                    }
                                }
                                SearchResultsListView.ItemsSource = searchResults;

                                if (searchResults.Count == 0)
                                {
                                    MessageBox.Show("No results found for the BaseSKU variants.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                return;
                            }
                        }
                    }

                    // General query excluding BaseSKUs
                    query = @"
            SELECT 
                i.CategoryID,
                i.SKU,
                c.ProductName AS Title,
                c.Price,
                i.QuantityOnHandSellable AS SellableQuantity,
                i.QuantityOnHandDefective AS DefectiveQuantity,
                i.QuantityOnHandSellable AS ExpectedSellableQuantity,
                i.QuantityOnHandDefective AS ExpectedDefectiveQuantity
            FROM Inventory i
            INNER JOIN Catalog c ON i.SKU = c.SKU
            WHERE i.LocationID = @LocationID AND c.IsBaseSKU = 0"; // Exclude BaseSKUs

                    // Add dynamic filters
                    if (!string.IsNullOrEmpty(sku))
                        query += " AND i.SKU LIKE @SKU";

                    if (!string.IsNullOrEmpty(category))
                        query += " AND i.CategoryID LIKE @CategoryID";

                    if (!string.IsNullOrEmpty(minPrice))
                        query += " AND c.Price >= @MinPrice";

                    if (!string.IsNullOrEmpty(maxPrice))
                        query += " AND c.Price <= @MaxPrice";

                    if (!string.IsNullOrEmpty(minQuantity))
                        query += " AND i.QuantityOnHandSellable >= @MinQuantity";

                    if (!string.IsNullOrEmpty(maxQuantity))
                        query += " AND i.QuantityOnHandSellable <= @MaxQuantity";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", locationID);

                        if (!string.IsNullOrEmpty(sku))
                            command.Parameters.AddWithValue("@SKU", $"%{sku}%");

                        if (!string.IsNullOrEmpty(category))
                            command.Parameters.AddWithValue("@CategoryID", $"%{category}%");

                        if (!string.IsNullOrEmpty(minPrice))
                            command.Parameters.AddWithValue("@MinPrice", Convert.ToDecimal(minPrice));

                        if (!string.IsNullOrEmpty(maxPrice))
                            command.Parameters.AddWithValue("@MaxPrice", Convert.ToDecimal(maxPrice));

                        if (!string.IsNullOrEmpty(minQuantity))
                            command.Parameters.AddWithValue("@MinQuantity", Convert.ToInt32(minQuantity));

                        if (!string.IsNullOrEmpty(maxQuantity))
                            command.Parameters.AddWithValue("@MaxQuantity", Convert.ToInt32(maxQuantity));

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                searchResults.Add(new SearchResultItem
                                {
                                    CategoryID = reader["CategoryID"].ToString(),
                                    SKU = reader["SKU"].ToString(),
                                    Title = reader["Title"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    SellableQuantity = Convert.ToInt32(reader["SellableQuantity"]),
                                    DefectiveQuantity = Convert.ToInt32(reader["DefectiveQuantity"]),
                                    ExpectedSellableQuantity = Convert.ToInt32(reader["ExpectedSellableQuantity"]),
                                    ExpectedDefectiveQuantity = Convert.ToInt32(reader["ExpectedDefectiveQuantity"])
                                });
                            }
                        }
                    }

                    SearchResultsListView.ItemsSource = searchResults;

                    if (searchResults.Count == 0)
                    {
                        MessageBox.Show("No results found for the given criteria.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Invalid input: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            // Optional: Implement sorting logic if needed
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Reset search filters
            SkuTextBox.Clear();
            CategoryTextBox.Clear();
            MinPriceTextBox.Clear();
            MaxPriceTextBox.Clear();
            MinQuantityTextBox.Clear();
            MaxQuantityTextBox.Clear();
            SearchResultsListView.ItemsSource = null;
        }
    }

    // Define the model for the search results
    public class SearchResultItem
    {
        public string CategoryID { get; set; }
        public string SKU { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public int SellableQuantity { get; set; }
        public int DefectiveQuantity { get; set; }
        public int ExpectedSellableQuantity { get; set; }
        public int ExpectedDefectiveQuantity { get; set; }
    }
}
