using MerlinBackOffice.Windows.InventoryWindows;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MerlinBackOffice.Menus;

using MerlinBackOffice.Helpers;

namespace MerlinBackOffice.Windows
{
    /// <summary>
    /// Interaction logic for InventoryMainWindow.xaml
    /// </summary>
    public partial class InventoryMainWindow : Window
    {

        private readonly string locationID = Properties.Settings.Default.LocationID; // Fetch the LocationID from settings
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming a DatabaseHelper class
        private bool showingFinalized = false; // To track whether we are showing finalized cartons

        public InventoryMainWindow()
        {
            InitializeComponent();
            LoadInboundCartons(new string[] { "IN TRANSIT" }); // Pass 'IN TRANSIT' as the default status
        }



        private void LoadInboundCartons(string[] statuses)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    // Adjust the query to filter by multiple statuses
                    string query = @"SELECT CartonID AS CCN, CartonOrigin AS Origin, CartonDestination AS Destination, 
                             CartonShipDate AS ShipDate, CartonReceiveDate AS ReceiveDate, 
                             CartonReceiveEmployee AS ReceiveEmployee, CartonStatus AS Status 
                             FROM Cartons 
                             WHERE CartonDestination = @LocationID AND CartonStatus IN (@Statuses)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID); // Use current location

                        // Convert array of statuses into a comma-separated string
                        string statusesString = string.Join(",", statuses.Select(s => $"'{s}'"));
                        cmd.CommandText = query.Replace("@Statuses", statusesString);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable cartonsTable = new DataTable();
                            adapter.Fill(cartonsTable);
                            ShippingGrid.ItemsSource = cartonsTable.DefaultView; // Bind the DataTable to the DataGrid
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ShippingGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShippingGrid.SelectedItem == null) return;

            DataRowView selectedCarton = (DataRowView)ShippingGrid.SelectedItem;
            string cartonID = selectedCarton["CCN"].ToString();

            // Prompt the user to enter the total number of items in the carton
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the total number of items in the carton:",
                "Verify Carton",
                "");

            if (int.TryParse(input, out int enteredTotalItems))
            {
                // Fetch the expected number of items in the carton from the Cartons table
                int expectedTotalItems = GetTotalItemsInCarton(cartonID);

                if (enteredTotalItems == expectedTotalItems)
                {
                    // If the totals match, update inventory
                    UpdateInventoryForCarton(cartonID);
                }
                else
                {
                    // If totals don't match, show the discrepancy window
                    CartonDiscrepancyWindow discrepancyWindow = new CartonDiscrepancyWindow(cartonID);
                    discrepancyWindow.ShowDialog(); // Open the window to handle the discrepancy
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to get the total number of items in the selected carton
        private int GetTotalItemsInCarton(string cartonID)
        {
            int totalItems = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT SUM(ProductQuantityShipped) FROM CartonDetails WHERE CartonID = @CartonID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);
                        totalItems = (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return totalItems;
        }

        // Method to update the sellable inventory for the carton
        private void UpdateInventoryForCarton(string cartonID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Fetch all the serialized items from the carton
                    string selectSerializedItemsQuery = @"
                SELECT cd.SKU, cd.ProductSerialNumber 
                FROM CartonDetails cd
                INNER JOIN Catalog c ON cd.SKU = c.SKU
                INNER JOIN CategoryTraits ct ON c.CategoryID = ct.CategoryID
                WHERE cd.CartonID = @CartonID AND ct.TraitName = 'Serialized'";

                    List<(string SKU, string SerialNumber)> serializedItems = new List<(string SKU, string SerialNumber)>();

                    using (SqlCommand cmd = new SqlCommand(selectSerializedItemsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string sku = reader["SKU"].ToString();
                                string serialNumber = reader["ProductSerialNumber"].ToString();
                                serializedItems.Add((sku, serialNumber.Trim())); // Ensure trimming of serial numbers to avoid issues with spaces
                            }
                        }
                    }

                    // Prompt for serial numbers if there are serialized items in the carton
                    if (serializedItems.Count > 0)
                    {
                        foreach (var item in serializedItems)
                        {
                            // Prompt the user to enter serial numbers for each serialized item
                            string inputSerialNumber = Microsoft.VisualBasic.Interaction.InputBox(
                                $"Enter the serial number for SKU: {item.SKU}",
                                "Enter Serial Number",
                                "");

                            if (string.IsNullOrWhiteSpace(inputSerialNumber))
                            {
                                MessageBox.Show($"Serial number is required for SKU: {item.SKU}.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return; // Exit if the serial number is not provided
                            }

                            // Trim both user input and database value, and compare case-insensitively
                            if (!string.Equals(inputSerialNumber.Trim(), item.SerialNumber.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                MessageBox.Show($"Serial number mismatch for SKU: {item.SKU}. Please try again.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return; // Exit if serial number validation fails
                            }
                        }
                    }

                    // Now, update inventory quantities as usual
                    string updateInventoryQuery = @"
                UPDATE Inventory 
                SET QuantityOnHandSellable = QuantityOnHandSellable + cd.ProductQuantityShipped
                FROM CartonDetails cd
                JOIN Inventory i ON i.SKU = cd.SKU
                WHERE cd.CartonID = @CartonID AND i.LocationID = @LocationID";

                    using (SqlCommand cmd = new SqlCommand(updateInventoryQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);
                        cmd.Parameters.AddWithValue("@LocationID", locationID); // LocationID is fetched from settings
                        cmd.ExecuteNonQuery();
                    }

                    // Now, finalize the carton by setting the receive date and updating status to 'FINALIZED'
                    string updateCartonQuery = @"
                UPDATE Cartons 
                SET CartonReceiveDate = @ReceiveDate, CartonReceiveEmployee = @EmployeeID, CartonStatus = 'FINALIZED'
                WHERE CartonID = @CartonID";

                    using (SqlCommand cmd = new SqlCommand(updateCartonQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);
                        cmd.Parameters.AddWithValue("@ReceiveDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@EmployeeID", Environment.UserName); // Adjust as needed to get employee info
                        cmd.ExecuteNonQuery();
                    }

                    // Insert the validated serial numbers into SerialNumberLog
                    foreach (var item in serializedItems)
                    {
                        string insertSerialLogQuery = @"
                    INSERT INTO SerialNumberLog (LocationID, SKU, SerialNumber)
                    VALUES (@LocationID, @SKU, @SerialNumber)";

                        using (SqlCommand cmd = new SqlCommand(insertSerialLogQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@LocationID", locationID);
                            cmd.Parameters.AddWithValue("@SKU", item.SKU);
                            cmd.Parameters.AddWithValue("@SerialNumber", item.SerialNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Inventory and carton finalized successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void OnEditCarton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure a carton is selected from the grid
            if (ShippingGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a carton to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get the selected carton from the DataGrid
            DataRowView selectedCarton = (DataRowView)ShippingGrid.SelectedItem;
            string cartonID = selectedCarton["CCN"].ToString();
            string cartonStatus = selectedCarton["Status"].ToString();

            // Check if the carton is finalized and prevent editing
            if (cartonStatus == "FINALIZED")
            {
                MessageBox.Show("This carton has been finalized and cannot be edited.", "Carton Finalized", MessageBoxButton.OK, MessageBoxImage.Information);
                return; // Exit the method to prevent any further actions
            }

            // If not finalized, proceed with the edit process
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the total number of items in the carton:",
                "Verify Carton",
                "");

            if (int.TryParse(input, out int enteredTotalItems))
            {
                // Fetch the expected number of items in the carton from the Cartons table
                int expectedTotalItems = GetTotalItemsInCarton(cartonID);

                if (enteredTotalItems == expectedTotalItems)
                {
                    // If the totals match, update inventory
                    UpdateInventoryForCarton(cartonID);
                }
                else
                {
                    // If totals don't match, show the discrepancy window
                    CartonDiscrepancyWindow discrepancyWindow = new CartonDiscrepancyWindow(cartonID);
                    discrepancyWindow.ShowDialog(); // Open the window to handle the discrepancy
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void OnViewFinalized_Click(object sender, RoutedEventArgs e)
        {
            if (showingFinalized)
            {
                // If currently showing finalized, switch back to only IN TRANSIT cartons
                LoadInboundCartons(new string[] { "IN TRANSIT" });
                btnViewFinalized.Content = "View Finalized";
                showingFinalized = false;
            }
            else
            {
                // Show both IN TRANSIT and FINALIZED cartons
                LoadInboundCartons(new string[] { "IN TRANSIT", "FINALIZED" });
                btnViewFinalized.Content = "View Unfinalized";
                showingFinalized = true;
            }
        }


        private void OnBtnInventoryAdjustments_Click(object sender, RoutedEventArgs e)
        {
            InventoryAdjustmentsMenu inventoryAdjustmentsMenu = new InventoryAdjustmentsMenu();
            inventoryAdjustmentsMenu.ShowDialog();
        }

        private void OnBtnCounts_Click(object sender, RoutedEventArgs e)
        {
            CountsMenu countsMenu = new CountsMenu();
            countsMenu.ShowDialog();
        }

        private void OnBtnPriceSearch_Click(object sender, RoutedEventArgs e)
        {
            PriceSearchWindow priceSearchWindow = new PriceSearchWindow();
            priceSearchWindow.ShowDialog();
        }
    }
}
