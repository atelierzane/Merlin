using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.VendorManagerPages
{
    public partial class VendorSearchPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public VendorSearchPage()
        {
            InitializeComponent();
        }

        // Search button event handler
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Collect input values from the search fields
            string vendorID = VendorIDTextBox.Text.Trim();
            string vendorName = VendorNameTextBox.Text.Trim();
            string vendorContact = VendorContactTextBox.Text.Trim();
            string vendorSalesRep = VendorSalesRepTextBox.Text.Trim();

            LoadVendors(vendorID, vendorName, vendorContact, vendorSalesRep);
        }

        // Reset button event handler
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear search fields
            VendorIDTextBox.Clear();
            VendorNameTextBox.Clear();
            VendorContactTextBox.Clear();
            VendorSalesRepTextBox.Clear();
            VendorDataGrid.ItemsSource = null; // Clear the data grid
        }

        // Method to load vendors based on search criteria
        private void LoadVendors(string vendorID, string vendorName, string vendorContact, string vendorSalesRep)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Construct query
                    string query = "SELECT VendorID, VendorName, VendorContact, VendorContactPhone, VendorContactEmail, " +
                                   "VendorSalesRep, VendorSalesRepPhone, VendorSalesRepEmail FROM Vendors WHERE 1=1"; // '1=1' allows us to append more conditions

                    // Append filtering conditions based on input
                    if (!string.IsNullOrEmpty(vendorID))
                        query += " AND VendorID = @VendorID";
                    if (!string.IsNullOrEmpty(vendorName))
                        query += " AND VendorName LIKE @VendorName";
                    if (!string.IsNullOrEmpty(vendorContact))
                        query += " AND VendorContact LIKE @VendorContact";
                    if (!string.IsNullOrEmpty(vendorSalesRep))
                        query += " AND VendorSalesRep LIKE @VendorSalesRep";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters to the query
                        if (!string.IsNullOrEmpty(vendorID))
                            cmd.Parameters.AddWithValue("@VendorID", vendorID);
                        if (!string.IsNullOrEmpty(vendorName))
                            cmd.Parameters.AddWithValue("@VendorName", $"%{vendorName}%");
                        if (!string.IsNullOrEmpty(vendorContact))
                            cmd.Parameters.AddWithValue("@VendorContact", $"%{vendorContact}%");
                        if (!string.IsNullOrEmpty(vendorSalesRep))
                            cmd.Parameters.AddWithValue("@VendorSalesRep", $"%{vendorSalesRep}%");

                        List<Vendor> vendors = new List<Vendor>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                vendors.Add(new Vendor
                                {
                                    VendorID = reader["VendorID"].ToString(),
                                    VendorName = reader["VendorName"].ToString(),
                                    VendorContact = reader["VendorContact"].ToString(),
                                    VendorContactPhone = reader["VendorContactPhone"].ToString(),
                                    VendorContactEmail = reader["VendorContactEmail"].ToString(),
                                    VendorSalesRep = reader["VendorSalesRep"].ToString(),
                                    VendorSalesRepPhone = reader["VendorSalesRepPhone"].ToString(),
                                    VendorSalesRepEmail = reader["VendorSalesRepEmail"].ToString(),
                                });
                            }
                        }

                        // Bind the results to the DataGrid
                        VendorDataGrid.ItemsSource = vendors;
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
