using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.VendorManagerPages
{
    public partial class AddVendorPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public AddVendorPage()
        {
            InitializeComponent();
        }

        private void SaveVendor_Click(object sender, RoutedEventArgs e)
        {
            // Collect input values
            string vendorID = VendorIDTextBox.Text.Trim();
            string vendorName = VendorNameTextBox.Text.Trim();
            string vendorContact = VendorContactTextBox.Text.Trim();
            string vendorContactPhone = VendorContactPhoneTextBox.Text.Trim();
            string vendorContactEmail = VendorContactEmailTextBox.Text.Trim();
            string vendorSalesRep = VendorSalesRepTextBox.Text.Trim();
            string vendorSalesRepPhone = VendorSalesRepPhoneTextBox.Text.Trim();
            string vendorSalesRepEmail = VendorSalesRepEmailTextBox.Text.Trim();

            // Validate VendorID is a 6-character string
            if (vendorID.Length != 6)
            {
                MessageBox.Show("Vendor ID must be 6 characters.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "INSERT INTO Vendors (VendorID, VendorName, VendorContact, VendorContactPhone, VendorContactEmail, " +
                                   "VendorSalesRep, VendorSalesRepPhone, VendorSalesRepEmail) " +
                                   "VALUES (@VendorID, @VendorName, @VendorContact, @VendorContactPhone, @VendorContactEmail, " +
                                   "@VendorSalesRep, @VendorSalesRepPhone, @VendorSalesRepEmail)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VendorID", vendorID);
                        cmd.Parameters.AddWithValue("@VendorName", vendorName);
                        cmd.Parameters.AddWithValue("@VendorContact", vendorContact);
                        cmd.Parameters.AddWithValue("@VendorContactPhone", vendorContactPhone);
                        cmd.Parameters.AddWithValue("@VendorContactEmail", vendorContactEmail);
                        cmd.Parameters.AddWithValue("@VendorSalesRep", vendorSalesRep);
                        cmd.Parameters.AddWithValue("@VendorSalesRepPhone", vendorSalesRepPhone);
                        cmd.Parameters.AddWithValue("@VendorSalesRepEmail", vendorSalesRepEmail);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Vendor added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
