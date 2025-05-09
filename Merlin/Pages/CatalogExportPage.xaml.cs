using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages
{
    /// <summary>
    /// Interaction logic for CatalogExportPage.xaml
    /// </summary>
    public partial class CatalogExportPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private List<Product> catalogData = new List<Product>();

        public CatalogExportPage()
        {
            InitializeComponent();
            LoadCatalog();
        }

        // Load and display the catalog in the DataGrid
        private void LoadCatalog()
        {
            catalogData = FetchCatalogData();
            CatalogPreviewGrid.ItemsSource = catalogData;
        }

        // Refresh button to reload catalog data
        private void btnRefreshCatalog_Click(object sender, RoutedEventArgs e)
        {
            LoadCatalog();
        }

        // Fetch all products from the catalog table
        private List<Product> FetchCatalogData()
        {
            List<Product> catalog = new List<Product>();

            using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
            {
                conn.Open();
                string query = @"
            SELECT c.SKU, c.ProductName, c.CategoryID, cm.CategoryName, c.Price, c.UPC
            FROM Catalog c
            LEFT JOIN CategoryMap cm ON c.CategoryID = cm.CategoryID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            catalog.Add(new Product
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                CategoryID = reader["CategoryID"].ToString(),
                                CategoryName = reader["CategoryName"] != DBNull.Value ? reader["CategoryName"].ToString() : "Uncategorized",
                                Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0,
                                UPC = reader["UPC"] != DBNull.Value ? reader["UPC"].ToString() : ""
                            });
                        }
                    }
                }
            }

            return catalog;
        }


        // Handle the Export Catalog button click
        private void btnExportCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (catalogData.Count == 0)
            {
                MessageBox.Show("No catalog data available for export.", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Export Catalog",
                FileName = "CatalogExport.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    WriteCatalogToCSV(catalogData, saveFileDialog.FileName);
                    MessageBox.Show("Catalog exported successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting catalog: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Write catalog data to a CSV file
        private void WriteCatalogToCSV(List<Product> catalog, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write CSV headers (with Category Name)
                writer.WriteLine("SKU,ProductName,CategoryID,CategoryName,Price,UPC");

                // Write each product row
                foreach (var product in catalog)
                {
                    writer.WriteLine($"{product.SKU},{EscapeCSV(product.ProductName)},{product.CategoryID},{EscapeCSV(product.CategoryName)},{product.Price.ToString("0.00")},{FormatUPC(product.UPC)}");
                }
            }
        }


        // Ensure UPCs retain leading zeros in Excel
        private string FormatUPC(string upc)
        {
            return string.IsNullOrEmpty(upc) ? "" : $"=\"{upc}\""; // Excel-safe formatting
        }


        // Escape special characters for CSV
        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Escape double quotes by replacing " with ""
            value = value.Replace("\"", "\"\"");

            // If value contains a comma, newline, or quote, wrap it in quotes
            if (value.Contains(",") || value.Contains("\n") || value.Contains("\""))
            {
                value = $"\"{value}\"";
            }

            return value;
        }

    }
}
