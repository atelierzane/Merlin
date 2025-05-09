using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.InventoryManagerPages
{
    public partial class CartonSearchPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming you have a DatabaseHelper class

        public CartonSearchPage()
        {
            InitializeComponent();
        }

        // Event handler for searching cartons
        private void SearchCartons_Click(object sender, RoutedEventArgs e)
        {
            string cartonID = CartonIDSearchTextBox.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CartonID, CartonOrigin, CartonDestination, CartonStatus, TotalItemsShipped FROM Cartons WHERE CartonID LIKE @CartonID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", $"%{cartonID}%"); // Allow partial matching of CartonID

                        List<Carton> cartons = new List<Carton>();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cartons.Add(new Carton
                                {
                                    CartonID = reader["CartonID"].ToString(),
                                    CartonOrigin = reader["CartonOrigin"].ToString(),
                                    CartonDestination = reader["CartonDestination"].ToString(),
                                    CartonStatus = reader["CartonStatus"].ToString(),
                                    TotalItemsShipped = (int)reader["TotalItemsShipped"]
                                });
                            }
                        }
                        CartonDataGrid.ItemsSource = cartons; // Bind carton data to DataGrid
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handler when a carton is selected
        private void CartonDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CartonDataGrid.SelectedItem is Carton selectedCarton)
            {
                LoadCartonDetails(selectedCarton.CartonID); // Load details of selected carton
            }
        }

        // Method to load details for the selected carton from CartonDetails table
        private void LoadCartonDetails(string cartonID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    // Modify query to join CartonDetails, Catalog, and CategoryMap tables
                    string query = @"
                SELECT cd.SKU, c.ProductName, c.CategoryID, cd.ProductSerialNumber, cd.ProductQuantityShipped, cd.ProductQuantityReceived
                FROM CartonDetails cd
                JOIN Catalog c ON cd.SKU = c.SKU
                JOIN CategoryMap cm ON c.CategoryID = cm.CategoryID
                WHERE cd.CartonID = @CartonID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);

                        List<CartonDetail> cartonDetails = new List<CartonDetail>();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cartonDetails.Add(new CartonDetail
                                {
                                    SKU = reader["SKU"].ToString(),
                                    ProductName = reader["ProductName"].ToString(), // Get the product name
                                    CategoryID = reader["CategoryID"].ToString(),   // Get the CategoryID
                                    ProductSerialNumber = reader["ProductSerialNumber"].ToString(),
                                    ProductQuantityShipped = (int)reader["ProductQuantityShipped"],
                                    ProductQuantityReceived = (int)reader["ProductQuantityReceived"]
                                });
                            }
                        }
                        CartonDetailsDataGrid.ItemsSource = cartonDetails; // Bind carton details to the DataGrid
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
