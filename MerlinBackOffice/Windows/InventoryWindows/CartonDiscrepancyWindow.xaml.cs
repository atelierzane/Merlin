using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using Microsoft.VisualBasic; // For InputBox
using MerlinBackOffice.Helpers;

namespace MerlinBackOffice.Windows.InventoryWindows
{
    public partial class CartonDiscrepancyWindow : Window
    {
        private readonly string cartonID;
        private readonly string locationID = Properties.Settings.Default.LocationID;
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public CartonDiscrepancyWindow(string cartonID)
        {
            InitializeComponent();
            this.cartonID = cartonID;
            LoadCartonDetails();
        }

        // Load the carton details into the DataGrid
        private void LoadCartonDetails()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        SELECT cd.SKU, c.ProductName, cd.ProductQuantityShipped, cd.ProductQuantityReceived 
                        FROM CartonDetails cd
                        JOIN Catalog c ON cd.SKU = c.SKU
                        WHERE cd.CartonID = @CartonID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable cartonDetailsTable = new DataTable();
                            adapter.Fill(cartonDetailsTable);
                            DiscrepancyGrid.ItemsSource = cartonDetailsTable.DefaultView;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method triggered by the Confirm button click
        private void ConfirmTotalReceived_Click(object sender, RoutedEventArgs e)
        {
            // Calculate total received quantity from the DataGrid
            DataTable cartonDetailsTable = ((DataView)DiscrepancyGrid.ItemsSource).ToTable();
            int totalReceivedItems = 0;

            foreach (DataRow row in cartonDetailsTable.Rows)
            {
                totalReceivedItems += Convert.ToInt32(row["ProductQuantityReceived"]);
            }

            // Prompt the user for the total number of items received
            string input = Interaction.InputBox("Enter the total number of items received:", "Confirm Total Received", "");

            if (int.TryParse(input, out int enteredTotalItems))
            {
                if (enteredTotalItems == totalReceivedItems)
                {
                    // If totals match, update the inventory
                    SaveUpdatedCartonDetails(cartonDetailsTable);
                    UpdateInventoryWithReceivedItems(cartonID);
                }
                else
                {
                    MessageBox.Show($"The entered total ({enteredTotalItems}) does not match the total of received items ({totalReceivedItems}).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveUpdatedCartonDetails(DataTable cartonDetailsTable)
        {
            bool hasDiscrepancies = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    foreach (DataRow row in cartonDetailsTable.Rows)
                    {
                        // Check if there is a discrepancy (quantity received differs from shipped)
                        if (Convert.ToInt32(row["ProductQuantityReceived"]) != Convert.ToInt32(row["ProductQuantityShipped"]))
                        {
                            hasDiscrepancies = true;
                        }

                        // Update the quantity received in CartonDetails
                        string updateDetailsQuery = "UPDATE CartonDetails SET ProductQuantityReceived = @Received WHERE CartonID = @CartonID AND SKU = @SKU";

                        using (SqlCommand cmd = new SqlCommand(updateDetailsQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Received", row["ProductQuantityReceived"]);
                            cmd.Parameters.AddWithValue("@CartonID", cartonID);
                            cmd.Parameters.AddWithValue("@SKU", row["SKU"]);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Now update the Cartons table with the finalized status, receive date, and discrepancy flag
                    string updateCartonQuery = @"
                UPDATE Cartons 
                SET CartonReceiveDate = @ReceiveDate, CartonReceiveEmployee = @EmployeeID, CartonStatus = 'FINALIZED', CartonHasDiscrepancy = @HasDiscrepancy
                WHERE CartonID = @CartonID";

                    using (SqlCommand cmd = new SqlCommand(updateCartonQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);
                        cmd.Parameters.AddWithValue("@ReceiveDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@EmployeeID", Environment.UserName); // Fetch the logged-in employee
                        cmd.Parameters.AddWithValue("@HasDiscrepancy", hasDiscrepancies ? 1 : 0); // Set discrepancy flag
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Carton details updated and finalized.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Method to update inventory based on the received items in the carton
        private void UpdateInventoryWithReceivedItems(string cartonID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        UPDATE Inventory 
                        SET QuantityOnHandSellable = QuantityOnHandSellable + cd.ProductQuantityReceived
                        FROM CartonDetails cd
                        JOIN Inventory i ON i.SKU = cd.SKU
                        WHERE cd.CartonID = @CartonID AND i.LocationID = @LocationID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);
                        cmd.Parameters.AddWithValue("@LocationID", locationID);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Inventory updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
