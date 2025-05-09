using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.ServicesManagerPages
{
    public partial class AddServiceFeePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public AddServiceFeePage()
        {
            InitializeComponent();
            LoadServices(); // Load existing services into the combo box
        }

        private void LoadServices()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ServiceID, ServiceName FROM Services";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ServiceComboBox.Items.Add(new Service
                                {
                                    ServiceID = reader["ServiceID"].ToString(),
                                    ServiceName = reader["ServiceName"].ToString()
                                });
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


        private void FeePricedByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FeePricedByComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Change the label based on the selected value
                if (selectedItem.Content.ToString() == "Percentage")
                {
                    FeePriceLabel.Text = "Fee Percentage:"; // Update the label
                }
                else
                {
                    FeePriceLabel.Text = "Fee Price:"; // Revert to the default label
                }
            }
        }

        private void SaveFeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a service.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string serviceID = ((dynamic)ServiceComboBox.SelectedItem).ServiceID;
            string feeName = FeeNameTextBox.Text.Trim();
            string feePriceText = FeePriceTextBox.Text.Trim();
            string feePricedBy = (FeePricedByComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            decimal feePrice;

            if (string.IsNullOrWhiteSpace(feeName))
            {
                MessageBox.Show("Fee Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(feePriceText, out feePrice) || feePrice <= 0)
            {
                MessageBox.Show("Please enter a valid fee price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ensure percentage fee logic is valid
            if (feePricedBy == "Percentage" && feePrice > 100)
            {
                MessageBox.Show("Percentage fees cannot exceed 100%.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string feeID = GenerateUniqueID("FEE");

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = "INSERT INTO ServiceFees (ServiceID, ServiceFeeID, ServiceFeeName, ServiceFeePrice, ServiceFeePricedBy) " +
                                   "VALUES (@ServiceID, @ServiceFeeID, @ServiceFeeName, @ServiceFeePrice, @ServiceFeePricedBy)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceID", serviceID);
                        cmd.Parameters.AddWithValue("@ServiceFeeID", feeID);
                        cmd.Parameters.AddWithValue("@ServiceFeeName", feeName);
                        cmd.Parameters.AddWithValue("@ServiceFeePrice", feePrice);
                        cmd.Parameters.AddWithValue("@ServiceFeePricedBy", feePricedBy);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Service Fee saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateUniqueID(string prefix)
        {
            Random random = new Random();
            return prefix + random.Next(100000, 999999).ToString();
        }

        private void ClearForm()
        {
            FeeNameTextBox.Clear();
            FeePriceTextBox.Clear();
            FeePricedByComboBox.SelectedIndex = -1;
            ServiceComboBox.SelectedIndex = -1;
        }
    }
}
