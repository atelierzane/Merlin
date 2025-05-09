using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.ServicesManagerPages
{
    public partial class AddServiceAddOnsPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public AddServiceAddOnsPage()
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

        private void SaveAddOnButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a service.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string serviceID = ((dynamic)ServiceComboBox.SelectedItem).ServiceID;
            string addOnName = AddOnNameTextBox.Text.Trim();
            string addOnPriceText = AddOnPriceTextBox.Text.Trim();
            string addOnPricedBy = (AddOnPricedByComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            decimal addOnPrice;

            if (string.IsNullOrWhiteSpace(addOnName))
            {
                MessageBox.Show("Add-On Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(addOnPriceText, out addOnPrice) || addOnPrice <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string addOnID = GenerateUniqueID("ADD");

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = "INSERT INTO ServiceAddOns (ServiceID, ServicePlusID, ServicePlusName, ServicePlusPrice, ServicePlusPricedBy) " +
                                   "VALUES (@ServiceID, @ServicePlusID, @ServicePlusName, @ServicePlusPrice, @ServicePlusPricedBy)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceID", serviceID);
                        cmd.Parameters.AddWithValue("@ServicePlusID", addOnID);
                        cmd.Parameters.AddWithValue("@ServicePlusName", addOnName);
                        cmd.Parameters.AddWithValue("@ServicePlusPrice", addOnPrice);
                        cmd.Parameters.AddWithValue("@ServicePlusPricedBy", addOnPricedBy);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Service Add-On saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            AddOnNameTextBox.Clear();
            AddOnPriceTextBox.Clear();
            AddOnPricedByComboBox.SelectedIndex = -1;
            ServiceComboBox.SelectedIndex = -1;
        }
    }
}
