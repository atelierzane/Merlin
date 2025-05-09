using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.VendorManagerPages
{
    public partial class EditVendorPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EditVendorPage()
        {
            InitializeComponent();
        }

        // Search for the vendor by Vendor ID
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
                    string query = "SELECT * FROM Vendors WHERE VendorID = @VendorID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VendorID", vendorID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                // Populate the fields with vendor data
                                VendorNameTextBox.Text = reader["VendorName"].ToString();
                                VendorContactTextBox.Text = reader["VendorContact"].ToString();
                                VendorContactPhoneTextBox.Text = reader["VendorContactPhone"].ToString();
                                VendorContactEmailTextBox.Text = reader["VendorContactEmail"].ToString();
                                VendorSalesRepTextBox.Text = reader["VendorSalesRep"].ToString();
                                VendorSalesRepPhoneTextBox.Text = reader["VendorSalesRepPhone"].ToString();
                                VendorSalesRepEmailTextBox.Text = reader["VendorSalesRepEmail"].ToString();

                                // Show the edit section
                                VendorEditSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No vendor found with the given Vendor ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                VendorEditSection.Visibility = Visibility.Collapsed;
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

        // Update the vendor information
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string vendorID = VendorIDTextBox.Text.Trim();
            string vendorName = VendorNameTextBox.Text.Trim();
            string vendorContact = VendorContactTextBox.Text.Trim();
            string vendorContactPhone = VendorContactPhoneTextBox.Text.Trim();
            string vendorContactEmail = VendorContactEmailTextBox.Text.Trim();
            string vendorSalesRep = VendorSalesRepTextBox.Text.Trim();
            string vendorSalesRepPhone = VendorSalesRepPhoneTextBox.Text.Trim();
            string vendorSalesRepEmail = VendorSalesRepEmailTextBox.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Vendors SET VendorName = @VendorName, VendorContact = @VendorContact, " +
                                         "VendorContactPhone = @VendorContactPhone, VendorContactEmail = @VendorContactEmail, " +
                                         "VendorSalesRep = @VendorSalesRep, VendorSalesRepPhone = @VendorSalesRepPhone, " +
                                         "VendorSalesRepEmail = @VendorSalesRepEmail WHERE VendorID = @VendorID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@VendorName", vendorName);
                        cmd.Parameters.AddWithValue("@VendorContact", vendorContact);
                        cmd.Parameters.AddWithValue("@VendorContactPhone", vendorContactPhone);
                        cmd.Parameters.AddWithValue("@VendorContactEmail", vendorContactEmail);
                        cmd.Parameters.AddWithValue("@VendorSalesRep", vendorSalesRep);
                        cmd.Parameters.AddWithValue("@VendorSalesRepPhone", vendorSalesRepPhone);
                        cmd.Parameters.AddWithValue("@VendorSalesRepEmail", vendorSalesRepEmail);
                        cmd.Parameters.AddWithValue("@VendorID", vendorID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Vendor updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the vendor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
