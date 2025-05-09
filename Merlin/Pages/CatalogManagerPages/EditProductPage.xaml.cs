using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.CatalogManagerPages
{
    public partial class EditProductPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming you have a DatabaseHelper class

        public EditProductPage()
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
                                // Populate the fields with the product data
                                ProductNameTextBox.Text = reader["ProductName"].ToString();
                                PriceTextBox.Text = reader["Price"].ToString();

                                // Show the edit fields
                                ProductEditSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No product found with the given SKU.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                ProductEditSection.Visibility = Visibility.Collapsed;
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

        // Update the product information
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string sku = SkuTextBox.Text.Trim();
            string productName = ProductNameTextBox.Text.Trim();
            string priceText = PriceTextBox.Text.Trim();

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(priceText))
            {
                MessageBox.Show("Please fill out all fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Catalog SET ProductName = @ProductName, Price = @Price WHERE SKU = @SKU";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", productName);
                        cmd.Parameters.AddWithValue("@Price", price);
                        cmd.Parameters.AddWithValue("@SKU", sku);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Product updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
