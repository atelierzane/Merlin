using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages
{
    /// <summary>
    /// Interaction logic for MigrationDashboard.xaml
    /// </summary>
    public partial class MigrationDashboard : Page
    {
        public MigrationDashboard()
        {
            InitializeComponent();
        }

        // Download Template Handlers
        private void DownloadCatalogTemplate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "CatalogTemplate",
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                string csvContent = "SKU,UPC,ProductName,CategoryID,Price,Variant1Name,Variant2Name,Variant3Name,Variant1Properties,Variant2Properties,Variant3Properties,IsBaseSKU,IsVariantSKU,VariantAssignedToBaseSKU\r\n" +
                                    ",012345678912,Example PS5 Game,128,59.99,Edition,Platform,,Standard,PlayStation 5,,1,0,";
                File.WriteAllText(filename, csvContent);
                AppendLog("Catalog template downloaded to " + filename);
            }
        }

        private void DownloadCategoryMapTemplate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "CategoryMapTemplate",
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                string csvContent = "CategoryID,CategoryName\r\n128,PS5 Games";
                File.WriteAllText(filename, csvContent);
                AppendLog("Category Map template downloaded to " + filename);
            }
        }

        private void DownloadLocationTemplate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "LocationTemplate",
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                string csvContent = "LocationID,LocationManagerID,LocationPhoneNumber,LocationStreetAddress,LocationCity,LocationState,LocationZIP,LocationType,LocationIsTradeHold,LocationTradeHoldDuration,LocationDistrictID,LocationRegionID,LocationMarketID,LocationDivisionID\r\n" +
                                    "LOC1,MGR001,123456789012,123 Main St.,Anytown,CA,90210,Retail,0,0,DIST01,REG01,MK1,DIV01";
                File.WriteAllText(filename, csvContent);
                AppendLog("Location template downloaded to " + filename);
            }
        }

        private void DownloadInventoryTemplate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "InventoryTemplate",
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                string csvContent = "SKU,UPC,LocationID,ProductName,CategoryID,QuantityOnHandSellable,QuantityOnHandDefective,InventoryStockAlert,InventoryStockAlertThreshold\r\n" +
                                    "SKU000001,012345678912,LOC1,Example PS5 Game,128,100,0,0,10";
                File.WriteAllText(filename, csvContent);
                AppendLog("Inventory template downloaded to " + filename);
            }
        }

        private void DownloadVendorsTemplate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "VendorsTemplate",
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                string csvContent = "VendorID (optional),VendorName,VendorContact,VendorSalesRep,VendorContactPhone,VendorSalesRepPhone,VendorContactEmail,VendorSalesRepEmail\r\n" +
                                    ",Example Vendor,John Doe,Jane Smith,1234567890,0987654321,john@example.com,jane@example.com";
                File.WriteAllText(filename, csvContent);
                AppendLog("Vendors template downloaded to " + filename);
            }
        }

        // Upload Handlers
        private void UploadCatalog_Click(object sender, RoutedEventArgs e)
        {
            string file = OpenCsvFile();
            if (!string.IsNullOrEmpty(file))
            {
                CatalogFileName.Text = System.IO.Path.GetFileName(file);
                AppendLog("Catalog file selected: " + file);
            }
        }

        private void UploadCategoryMap_Click(object sender, RoutedEventArgs e)
        {
            string file = OpenCsvFile();
            if (!string.IsNullOrEmpty(file))
            {
                CategoryMapFileName.Text = System.IO.Path.GetFileName(file);
                AppendLog("Category Map file selected: " + file);
            }
        }

        private void UploadLocation_Click(object sender, RoutedEventArgs e)
        {
            string file = OpenCsvFile();
            if (!string.IsNullOrEmpty(file))
            {
                LocationFileName.Text = System.IO.Path.GetFileName(file);
                AppendLog("Location file selected: " + file);
            }
        }

        private void UploadInventory_Click(object sender, RoutedEventArgs e)
        {
            string file = OpenCsvFile();
            if (!string.IsNullOrEmpty(file))
            {
                InventoryFileName.Text = System.IO.Path.GetFileName(file);
                AppendLog("Inventory file selected: " + file);
            }
        }

        private void UploadVendors_Click(object sender, RoutedEventArgs e)
        {
            string file = OpenCsvFile();
            if (!string.IsNullOrEmpty(file))
            {
                VendorsFileName.Text = System.IO.Path.GetFileName(file);
                AppendLog("Vendors file selected: " + file);
            }
        }

        // Import Data Handler
        private void ImportData_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("Starting data import...");

            // Here you would add your logic to read the CSV files and process the data.
            // For this example, we'll simulate a delay to represent processing.
            Thread.Sleep(1000);
            AppendLog("Data import completed successfully.");
        }

        // Helper method to open CSV file
        private string OpenCsvFile()
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv"
            };
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileName;
            }
            return string.Empty;
        }

        // Append messages to the Migration Log
        private void AppendLog(string message)
        {
            MigrationLogTextBox.AppendText($"{DateTime.Now}: {message}\r\n");
            MigrationLogTextBox.ScrollToEnd();
        }
    }
}
