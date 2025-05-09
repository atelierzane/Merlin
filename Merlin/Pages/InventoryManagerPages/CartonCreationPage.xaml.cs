using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.InventoryManagerPages
{
    public partial class CartonCreationPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming you have a DatabaseHelper class
        private List<CartonItem> cartonItems = new List<CartonItem>(); // List to hold items to be added to the carton
        private List<string> serialNumbers = new List<string>(); // List to hold serial numbers for serialized items

        public CartonCreationPage()
        {
            InitializeComponent();
            LoadOriginsAndDestinations(); // Load origin and destination locations
            CartonItemsDataGrid.ItemsSource = cartonItems; // Bind the DataGrid to the cartonItems list
        }

        // Load locations into ComboBoxes for origin and destination

        private void LoadOriginsAndDestinations()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Load Locations into both Origin and Destination
                    string locationQuery = "SELECT LocationID FROM Location";
                    using (SqlCommand cmd = new SqlCommand(locationQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string locationId = reader["LocationID"].ToString();
                                ComboBoxItem originItem = new ComboBoxItem { Content = $"LOC: {locationId}", Tag = locationId };
                                ComboBoxItem destinationItem = new ComboBoxItem { Content = locationId, Tag = locationId };

                                OriginComboBox.Items.Add(originItem);
                                DestinationComboBox.Items.Add(destinationItem);
                            }
                        }
                    }

                    // Load Vendors into Origin only
                    string vendorQuery = "SELECT VendorID FROM Vendors";
                    using (SqlCommand cmd = new SqlCommand(vendorQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string vendorId = reader["VendorID"].ToString();
                                ComboBoxItem originItem = new ComboBoxItem { Content = $"VEN: {vendorId}", Tag = vendorId };
                                OriginComboBox.Items.Add(originItem);
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


        // Helper method to check if the category of the item is serialized
        private bool IsCategorySerialized(string sku)
        {
            bool isSerialized = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    // Query to check if the item's category has the Serialized trait
                    string query = @"SELECT COUNT(*) 
                                     FROM CategoryTraits CT 
                                     INNER JOIN Catalog C ON C.CategoryID = CT.CategoryID
                                     WHERE C.SKU = @SKU AND CT.TraitName = 'Serialized'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SKU", sku);
                        int result = (int)cmd.ExecuteScalar();
                        isSerialized = result > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error while checking serialization: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return isSerialized;
        }

        private void AddProductToCarton_Click(object sender, RoutedEventArgs e)
        {
            string sku = ProductSkuTextBox.Text;
            if (string.IsNullOrWhiteSpace(sku))
            {
                MessageBox.Show("Please enter a valid SKU.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if the SKU is a BaseSKU
            if (IsBaseSKU(sku))
            {
                MessageBox.Show("Base SKUs cannot be added to cartons.", "Invalid SKU", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (int.TryParse(ProductQuantityTextBox.Text, out int quantityShipped))
            {
                // Check if the category associated with this SKU is serialized
                bool isSerialized = IsCategorySerialized(sku);

                if (isSerialized)
                {
                    // Show the serial number input section for serialized items
                    SerialNumberSection.Visibility = Visibility.Visible;
                    MessageBox.Show($"This product requires {quantityShipped} serial numbers. Please enter them.");
                    // Store SKU and quantity temporarily until serial numbers are added
                    ProductSkuTextBox.Tag = sku;
                    ProductQuantityTextBox.Tag = quantityShipped;
                    return; // Wait for serial numbers to be added before proceeding
                }

                // Add non-serialized items immediately
                cartonItems.Add(new CartonItem
                {
                    SKU = sku,
                    ProductSerialNumber = null, // No serial number for non-serialized items
                    ProductQuantityShipped = quantityShipped
                });

                // Refresh the DataGrid to show the new item
                CartonItemsDataGrid.Items.Refresh();

                // Clear the input fields for the next entry
                ProductSkuTextBox.Clear();
                ProductQuantityTextBox.Clear();
                SerialNumberListBox.Items.Clear();
                serialNumbers.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a valid quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        // Event handler for adding serial numbers to the list
        private void AddSerialNumber_Click(object sender, RoutedEventArgs e)
        {
            string serialNumber = SerialNumberInput.Text.Trim();
            if (!string.IsNullOrEmpty(serialNumber))
            {
                // Add the serial number to the list
                serialNumbers.Add(serialNumber);
                SerialNumberListBox.Items.Add(serialNumber);
                SerialNumberInput.Clear();
            }

            // Check if we've entered enough serial numbers
            if (serialNumbers.Count == (int)ProductQuantityTextBox.Tag)
            {
                // Add serialized items to the cartonItems list, one entry per serial number
                string sku = (string)ProductSkuTextBox.Tag;
                foreach (var serial in serialNumbers)
                {
                    cartonItems.Add(new CartonItem
                    {
                        SKU = sku,
                        ProductSerialNumber = serial,  // Capture the serial number
                        ProductQuantityShipped = 1 // Each serialized item is counted as 1
                    });
                }

                // Refresh the DataGrid to show the new serialized items
                CartonItemsDataGrid.Items.Refresh();

                // Hide the serial number input section and clear for the next product
                SerialNumberSection.Visibility = Visibility.Collapsed;
                ProductSkuTextBox.Clear();
                ProductQuantityTextBox.Clear();
                serialNumbers.Clear();
                SerialNumberListBox.Items.Clear();
            }
        }


        // Event handler for submitting the carton
        private void SubmitCarton_Click(object sender, RoutedEventArgs e)
        {
            string origin = (OriginComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();
            string destination = (DestinationComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();
            string carrier = CarrierTextBox.Text;
            string trackingNumber = TrackingNumberTextBox.Text;
            DateTime shipDate = ShipDatePicker.SelectedDate ?? DateTime.Now;

            // Validate input
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate serial numbers if serialized item is being added
            if (serialNumbers.Count == 0 && SerialNumberSection.Visibility == Visibility.Visible)
            {
                MessageBox.Show("Please enter serial numbers for the serialized items.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Generate a unique CartonID starting with the Destination LocationID
            string cartonID = destination.PadLeft(4, '0') + Guid.NewGuid().ToString("N").Substring(0, 21);

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Verify if cartonItems contains valid data
                    if (cartonItems.Count == 0)
                    {
                        MessageBox.Show("No items added to the carton.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Calculate the total quantity shipped across all items
                    int totalItemsShipped = cartonItems.Sum(item => item.ProductQuantityShipped);

                    if (totalItemsShipped == 0)
                    {
                        MessageBox.Show("Total quantity shipped cannot be zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Insert into Cartons table
                    string insertCartonQuery = @"INSERT INTO Cartons 
            (CartonID, CartonOrigin, CartonDestination, CartonCarrier, CartonTrackingNumber, CartonShipDate, CartonStatus, TotalItemsShipped, TotalItemsReceived) 
            VALUES (@CartonID, @Origin, @Destination, @Carrier, @TrackingNumber, @ShipDate, 'IN TRANSIT', @TotalItemsShipped, 0)";

                    using (SqlCommand cmd = new SqlCommand(insertCartonQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);
                        cmd.Parameters.AddWithValue("@Origin", origin);
                        cmd.Parameters.AddWithValue("@Destination", destination);
                        cmd.Parameters.AddWithValue("@Carrier", (object)carrier ?? DBNull.Value); // Allow carrier to be nullable
                        cmd.Parameters.AddWithValue("@TrackingNumber", (object)trackingNumber ?? DBNull.Value); // Allow tracking number to be nullable
                        cmd.Parameters.AddWithValue("@ShipDate", shipDate);
                        cmd.Parameters.AddWithValue("@TotalItemsShipped", totalItemsShipped); // Total quantity of items shipped
                        cmd.ExecuteNonQuery();
                    }

                    // Insert items into CartonDetails table, including serial numbers for serialized items
                    foreach (CartonItem item in cartonItems)
                    {
                        string insertDetailQuery = @"INSERT INTO CartonDetails 
                (CartonID, CartonOrigin, CartonDestination, SKU, ProductSerialNumber, ProductQuantityShipped, ProductQuantityReceived) 
                VALUES (@CartonID, @Origin, @Destination, @SKU, @SerialNumber, @QuantityShipped, 0)"; // Default ProductQuantityReceived to 0

                        using (SqlCommand cmd = new SqlCommand(insertDetailQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@CartonID", cartonID);
                            cmd.Parameters.AddWithValue("@Origin", origin);
                            cmd.Parameters.AddWithValue("@Destination", destination);
                            cmd.Parameters.AddWithValue("@SKU", item.SKU);
                            cmd.Parameters.AddWithValue("@SerialNumber", (object)item.ProductSerialNumber ?? DBNull.Value); // Handle serial numbers
                            cmd.Parameters.AddWithValue("@QuantityShipped", item.ProductQuantityShipped);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Carton created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsBaseSKU(string sku)
        {
            bool isBaseSKU = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    // Query to check if the SKU is a BaseSKU
                    string query = "SELECT IsBaseSKU FROM Catalog WHERE SKU = @SKU";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SKU", sku);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            isBaseSKU = Convert.ToBoolean(result);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error while checking BaseSKU: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return isBaseSKU;
        }


    }
}
