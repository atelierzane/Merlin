using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages
{
    /// <summary>
    /// Interaction logic for DataMigrationPage.xaml
    /// </summary>
    public partial class DataMigrationPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public DataMigrationPage()
        {
            InitializeComponent();
            LoadCategories();
            DataContext = this; // Ensures the DataGrid can access Categories
        }

        // Load categories into the ComboBox
        public List<Category> Categories { get; set; } = new List<Category>();

        private void LoadCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName FROM CategoryMap";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Categories.Clear(); // Reset list before adding categories

                        while (reader.Read())
                        {
                            Categories.Add(new Category
                            {
                                CategoryID = reader["CategoryID"].ToString(),
                                CategoryName = reader["CategoryName"].ToString()
                            });
                        }

                        BulkCategoryComboBox.ItemsSource = Categories;


                        BulkCategoryComboBox.DisplayMemberPath = "CategoryName";


                        BulkCategoryComboBox.SelectedValuePath = "CategoryID";

                        ProductPreviewGrid.Items.Refresh(); // Ensure DataGrid updates
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Open file dialog and load CSV data
        private void btnImportFile_Click(object sender, RoutedEventArgs e)
        {
            if (Categories.Count == 0)
            {
                LoadCategories(); // Ensure categories are loaded before importing
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Select Product Catalog File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                List<Product> importedProducts = LoadProductsFromCSV(openFileDialog.FileName);
                ProductPreviewGrid.ItemsSource = importedProducts;
            }
        }


        // Read CSV data into Product list
        private List<Product> LoadProductsFromCSV(string filePath)
        {
            List<Product> products = new List<Product>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                bool isFirstLine = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirstLine) { isFirstLine = false; continue; }

                    string[] values = line.Split(',');

                    if (values.Length < 4) continue;

                    string productName = values[0].Trim();
                    string categoryID = values[1].Trim();
                    string priceText = values[2].Trim();
                    string upc = values[3].Trim();

                    // Validate UPC: must be exactly 12 digits if present
                    if (!string.IsNullOrEmpty(upc) && (!upc.All(char.IsDigit) || upc.Length != 12))
                    {
                        upc = null; // Discard invalid UPCs
                    }

                    // Ensure valid price
                    decimal price = decimal.TryParse(priceText, out decimal parsedPrice) ? parsedPrice : 0;

                    products.Add(new Product
                    {
                        SKU = null,  // Will be generated later
                        ProductName = productName,
                        CategoryID = string.IsNullOrEmpty(categoryID) ? null : categoryID, // Allow empty category
                        Price = price,
                        UPC = upc // Keep only valid UPCs
                    });
                }
            }

            return products;
        }


        // Generate a unique SKU for a product
        private string GenerateSKU(string categoryID, SqlConnection conn)
        {
            Random random = new Random();
            string sku;
            int attempts = 0;

            do
            {
                string randomDigits = random.Next(100000, 999999).ToString();
                sku = categoryID + randomDigits;

                using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Catalog WHERE SKU = @SKU", conn))
                {
                    checkCmd.Parameters.AddWithValue("@SKU", sku);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count == 0) return sku;
                }

                attempts++;
            } while (attempts < 100);

            throw new Exception("Failed to generate a unique SKU.");
        }

        // Process the imported data and insert into the database
        private void ProcessImport(List<Product> products)
        {
            using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
            {
                conn.Open();

                foreach (var product in products)
                {
                    if (string.IsNullOrEmpty(product.CategoryID))
                    {
                        MessageBox.Show($"Product '{product.ProductName}' is missing a category. Please select one before importing.",
                                        "Missing Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (string.IsNullOrEmpty(product.SKU))
                        product.SKU = GenerateSKU(product.CategoryID, conn);

                    string insertQuery = @"
                INSERT INTO Catalog (SKU, ProductName, CategoryID, Price, UPC, IsBaseSKU, IsVariantSKU) 
                VALUES (@SKU, @ProductName, @CategoryID, @Price, @UPC, 0, 0)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SKU", product.SKU);
                        cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                        cmd.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                        cmd.Parameters.AddWithValue("@Price", product.Price);
                        cmd.Parameters.AddWithValue("@UPC", string.IsNullOrEmpty(product.UPC) ? DBNull.Value : (object)product.UPC);
                        cmd.ExecuteNonQuery();
                    }

                    AddToInventory(conn, product.SKU, product.CategoryID);
                }
            }

            MessageBox.Show("Data import completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        // Ensure inventory entries exist for imported products
        private void AddToInventory(SqlConnection conn, string sku, string categoryID)
        {
            string insertInventoryQuery = @"
                INSERT INTO Inventory (SKU, CategoryID, LocationID, QuantityOnHandSellable, QuantityOnHandDefective) 
                SELECT @SKU, @CategoryID, LocationID, 0, 0 FROM Location";

            using (SqlCommand cmd = new SqlCommand(insertInventoryQuery, conn))
            {
                cmd.Parameters.AddWithValue("@SKU", sku);
                cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                cmd.ExecuteNonQuery();
            }
        }

        // Handle import process button click
        private void btnProcessImport_Click(object sender, RoutedEventArgs e)
        {
            if (ProductPreviewGrid.ItemsSource is List<Product> products && products.Any())
            {
                ProcessImport(products);
            }
            else
            {
                MessageBox.Show("No products to import.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDownloadTemplate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Save Template File",
                FileName = "ProductImportTemplate.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Provide instructions to users on how to preserve UPCs correctly
                        writer.WriteLine("ProductName,CategoryID,Price,UPC");
                        writer.WriteLine("Example Product 1,115,29.99,012345678912");
                        writer.WriteLine("Example Product 2,120,59.99,098765432109");
                        writer.WriteLine("Example Product 3,,19.99,"); // No CategoryID & blank UPC example
                    }

                    MessageBox.Show("Template downloaded successfully.\n\n⚠ IMPORTANT: When opening in Excel, format the UPC column as 'Text' to avoid losing leading zeros.",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is Category selectedCategory)
            {
                // Ensure at least one row is selected
                if (ProductPreviewGrid.SelectedItems.Count > 0)
                {
                    // Get all selected products
                    var selectedProducts = ProductPreviewGrid.SelectedItems.Cast<Product>().ToList();

                    // Apply the selected category to each selected product
                    foreach (var product in selectedProducts)
                    {
                        if (product != null) // Ensure object is valid
                        {
                            product.CategoryID = selectedCategory.CategoryID;
                        }
                    }

                    // Refresh DataGrid UI on the main thread
                    Dispatcher.Invoke(() => ProductPreviewGrid.Items.Refresh());
                }
            }
        }


        private void btnApplyCategory_Click(object sender, RoutedEventArgs e)
        {
            // Ensure a category is selected
            if (BulkCategoryComboBox.SelectedItem is Category selectedCategory)
            {
                // Ensure at least one row is selected
                if (ProductPreviewGrid.SelectedItems.Count > 0)
                {
                    // Get all selected products
                    var selectedProducts = ProductPreviewGrid.SelectedItems.Cast<Product>().ToList();

                    // Apply the selected category to each selected product
                    foreach (var product in selectedProducts)
                    {
                        if (product != null) // Ensure object is valid
                        {
                            product.CategoryID = selectedCategory.CategoryID;
                        }
                    }

                    // Refresh DataGrid UI on the main thread
                    Dispatcher.Invoke(() => ProductPreviewGrid.Items.Refresh());
                }
                else
                {
                    MessageBox.Show("No items selected.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a category before applying.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // Handle cancel button click
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}
