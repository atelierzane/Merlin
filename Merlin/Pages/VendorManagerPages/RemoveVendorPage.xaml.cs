using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.VendorManagerPages
{
    public partial class RemoveVendorPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public RemoveVendorPage()
        {
            InitializeComponent();
        }

        // Search for the vendor by VendorID
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string vendorID = VendorIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(vendorID))
            {
                MessageBox.Show("Please enter a Vendor ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT VendorName, VendorContact, VendorContactPhone, VendorContactEmail, " +
                                   "VendorSalesRep, VendorSalesRepPhone, VendorSalesRepEmail " +
                                   "FROM Vendors WHERE VendorID = @VendorID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VendorID", vendorID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                // Populate the vendor information fields
                                VendorNameTextBlock.Text = reader["VendorName"].ToString();
                                VendorContactTextBlock.Text = reader["VendorContact"].ToString();
                                VendorContactPhoneTextBlock.Text = reader["VendorContactPhone"].ToString();
                                VendorContactEmailTextBlock.Text = reader["VendorContactEmail"].ToString();
                                VendorSalesRepTextBlock.Text = reader["VendorSalesRep"].ToString();
                                VendorSalesRepPhoneTextBlock.Text = reader["VendorSalesRepPhone"].ToString();
                                VendorSalesRepEmailTextBlock.Text = reader["VendorSalesRepEmail"].ToString();

                                // Show the vendor information section
                                VendorInfoSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No vendor found with the given Vendor ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                VendorInfoSection.Visibility = Visibility.Collapsed;
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

        // Delete the vendor
        private void DeleteVendor_Click(object sender, RoutedEventArgs e)
        {
            string vendorID = VendorIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(vendorID))
            {
                MessageBox.Show("Please search for a vendor first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete Vendor ID: {vendorID}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM Vendors WHERE VendorID = @VendorID";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@VendorID", vendorID);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Vendor deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                // Clear the fields
                                VendorIDTextBox.Clear();
                                VendorNameTextBlock.Text = string.Empty;
                                VendorContactTextBlock.Text = string.Empty;
                                VendorContactPhoneTextBlock.Text = string.Empty;
                                VendorContactEmailTextBlock.Text = string.Empty;
                                VendorSalesRepTextBlock.Text = string.Empty;
                                VendorSalesRepPhoneTextBlock.Text = string.Empty;
                                VendorSalesRepEmailTextBlock.Text = string.Empty;
                                VendorInfoSection.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete the vendor. Vendor ID not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
}
