using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.CatalogManagerPages
{
    public partial class RemoveProductPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming you have a DatabaseHelper class

        public RemoveProductPage()
        {
            InitializeComponent();
        }

        // Search for the product by SKU
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string sku = SkuTextBox.Text.Trim();

            if (string.IsNullOrEmpty(sku))
            {
                MessageBox.Show("Please enter a SKU.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ProductName, Price FROM Catalog WHERE SKU = @SKU";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SKU", sku);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                // Display the product information to confirm deletion
                                ProductNameTextBlock.Text = reader["ProductName"].ToString();
                                PriceTextBlock.Text = reader["Price"].ToString();

                                // Show the delete button section
                                ProductInfoSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No product found with the given SKU.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                ProductInfoSection.Visibility = Visibility.Collapsed;
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

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            string sku = SkuTextBox.Text.Trim();

            if (string.IsNullOrEmpty(sku))
            {
                MessageBox.Show("Please enter a SKU.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to remove this product?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();

                        // Begin transaction for consistency
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                // Delete product from the Catalog
                                string deleteCatalogQuery = "DELETE FROM Catalog WHERE SKU = @SKU";
                                using (SqlCommand deleteCatalogCmd = new SqlCommand(deleteCatalogQuery, conn, transaction))
                                {
                                    deleteCatalogCmd.Parameters.AddWithValue("@SKU", sku);
                                    int catalogRowsAffected = deleteCatalogCmd.ExecuteNonQuery();

                                    if (catalogRowsAffected == 0)
                                    {
                                        throw new Exception("Failed to remove the product from the catalog.");
                                    }
                                }

                                // Delete product from the Inventory for all locations
                                string deleteInventoryQuery = "DELETE FROM Inventory WHERE SKU = @SKU";
                                using (SqlCommand deleteInventoryCmd = new SqlCommand(deleteInventoryQuery, conn, transaction))
                                {
                                    deleteInventoryCmd.Parameters.AddWithValue("@SKU", sku);
                                    deleteInventoryCmd.ExecuteNonQuery();
                                }

                                // Commit the transaction if everything is successful
                                transaction.Commit();
                                MessageBox.Show("Product removed successfully from the catalog and inventory.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Hide the product info and clear the input fields
                                ProductInfoSection.Visibility = Visibility.Collapsed;
                                SkuTextBox.Clear();
                            }
                            catch (Exception ex)
                            {
                                // Rollback transaction if there was an error
                                transaction.Rollback();
                                MessageBox.Show($"Error removing the product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
