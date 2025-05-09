using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class AddComboPage : Page
    {
        public DatabaseHelper databaseHelper = new DatabaseHelper();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        public AddComboPage()
        {
            InitializeComponent();
            CategoryComboBox.ItemsSource = Categories; // Bind the ComboBox to Categories
        }

        // Ensure the UI is fully loaded before handling the visibility of panels
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories(); // Load categories for placeholders

            // Initialize the visibility of panels after the page has fully loaded
            if (rbSku.IsChecked == true)
            {
                SkuPanel.Visibility = Visibility.Visible;
                CategoryPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                SkuPanel.Visibility = Visibility.Collapsed;
                CategoryPanel.Visibility = Visibility.Visible;
            }
        }

        // Switch between SKU and Category input based on the selected option
        private void ComboOption_Checked(object sender, RoutedEventArgs e)
        {
            // Safeguard against null reference
            if (SkuPanel != null && CategoryPanel != null)
            {
                if (rbSku.IsChecked == true)
                {
                    SkuPanel.Visibility = Visibility.Visible;
                    CategoryPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SkuPanel.Visibility = Visibility.Collapsed;
                    CategoryPanel.Visibility = Visibility.Visible;
                }
            }
        }

        // Add SKU to combo list
        private void AddSkuToCombo_Click(object sender, RoutedEventArgs e)
        {
            string sku = SkuTextBox.Text.Trim();

            // Get product details from catalog
            Product product = GetProductFromCatalog(sku);

            if (product == null)
            {
                MessageBox.Show("The SKU entered does not exist in the catalog.", "Invalid SKU", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add product to combo items list
            ComboItemsListBox.Items.Add(new ComboItem
            {
                SKU = product.SKU,
                ProductName = product.ProductName,
                CategoryID = product.CategoryID,
                CategoryName = product.CategoryName,
                Quantity = quantity
            });

            SkuTextBox.Clear();
            QuantityTextBox.Clear();
        }

        // Add category placeholder to combo list
        private void AddCategoryToCombo_Click(object sender, RoutedEventArgs e)
        {
            string categoryID = (string)CategoryComboBox.SelectedValue;

            if (string.IsNullOrEmpty(categoryID))
            {
                MessageBox.Show("Please select a valid category.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityPlaceholderTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add category placeholder to combo items list
            ComboItemsListBox.Items.Add(new ComboItem
            {
                SKU = null,  // Placeholder, no specific SKU
                CategoryID = categoryID,
                CategoryName = ((Category)CategoryComboBox.SelectedItem).CategoryName,
                Quantity = quantity
            });

            QuantityPlaceholderTextBox.Clear();
        }

        // Save combo and insert records into database
        private void SaveCombo_Click(object sender, RoutedEventArgs e)
        {
            string comboName = ComboNameTextBox.Text.Trim();
            if (!decimal.TryParse(ComboPriceTextBox.Text, out decimal comboPrice) || comboPrice <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Generate a unique ComboSKU
                    string comboSKU = GenerateUniqueComboSKU(conn);

                    // Insert combo into Combos table
                    string insertComboQuery = "INSERT INTO Combos (ComboSKU, ComboName, ComboPrice) VALUES (@ComboSKU, @ComboName, @ComboPrice)";
                    using (SqlCommand cmd = new SqlCommand(insertComboQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ComboSKU", comboSKU);
                        cmd.Parameters.AddWithValue("@ComboName", comboName);
                        cmd.Parameters.AddWithValue("@ComboPrice", comboPrice);
                        cmd.ExecuteNonQuery();
                    }

                    // Insert combo items into ComboItems table
                    foreach (ComboItem item in ComboItemsListBox.Items)
                    {
                        string insertComboItemQuery = string.IsNullOrEmpty(item.SKU)
                            ? "INSERT INTO ComboItems (ComboSKU, CategoryID, Quantity) VALUES (@ComboSKU, @CategoryID, @Quantity)"
                            : "INSERT INTO ComboItems (ComboSKU, ProductSKU, Quantity) VALUES (@ComboSKU, @ProductSKU, @Quantity)";
                        using (SqlCommand itemCmd = new SqlCommand(insertComboItemQuery, conn))
                        {
                            itemCmd.Parameters.AddWithValue("@ComboSKU", comboSKU);
                            if (!string.IsNullOrEmpty(item.SKU))
                                itemCmd.Parameters.AddWithValue("@ProductSKU", item.SKU);
                            else
                                itemCmd.Parameters.AddWithValue("@CategoryID", item.CategoryID);
                            itemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            itemCmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Combo saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Generate unique ComboSKU
        private string GenerateUniqueComboSKU(SqlConnection conn)
        {
            string comboSKU = null;
            Random random = new Random();
            bool unique = false;

            while (!unique)
            {
                comboSKU = "202" + random.Next(100000, 999999).ToString();
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Combos WHERE ComboSKU = @ComboSKU", conn))
                {
                    cmd.Parameters.AddWithValue("@ComboSKU", comboSKU);
                    int count = (int)cmd.ExecuteScalar();
                    if (count == 0) unique = true;
                }
            }

            return comboSKU;
        }

        // Retrieve product details from catalog by SKU
        private Product GetProductFromCatalog(string sku)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        SELECT C.SKU, C.ProductName, C.CategoryID, CM.CategoryName
                        FROM Catalog C
                        JOIN CategoryMap CM ON C.CategoryID = CM.CategoryID
                        WHERE C.SKU = @SKU";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SKU", sku);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Product
                                {
                                    SKU = reader["SKU"].ToString(),
                                    ProductName = reader["ProductName"].ToString(),
                                    CategoryID = reader["CategoryID"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        // Load categories into CategoryComboBox
        private void LoadCategories()
        {
            try
            {
                Categories.Clear(); // Clear any existing categories before loading new ones

                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName FROM CategoryMap";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Categories.Add(new Category
                                {
                                    CategoryID = reader["CategoryID"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
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
    }
}
