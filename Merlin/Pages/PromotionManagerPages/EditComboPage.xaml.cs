using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class EditComboPage : Page
    {
        public DatabaseHelper databaseHelper = new DatabaseHelper();

        public EditComboPage()
        {
            InitializeComponent();
        }

        // Search combo by ComboSKU
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string comboSKU = SkuTextBox.Text.Trim();

            if (string.IsNullOrEmpty(comboSKU))
            {
                MessageBox.Show("Please enter a Combo SKU.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ComboName, ComboPrice FROM Combos WHERE ComboSKU = @ComboSKU";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ComboSKU", comboSKU);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                ComboNameTextBox.Text = reader["ComboName"].ToString();
                                PriceTextBox.Text = reader["ComboPrice"].ToString();

                                ComboEditSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No combo found with the given SKU.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                ComboEditSection.Visibility = Visibility.Collapsed;
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

        // Update the combo details
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string comboSKU = SkuTextBox.Text.Trim();
            string comboName = ComboNameTextBox.Text.Trim();
            string priceText = PriceTextBox.Text.Trim();

            if (string.IsNullOrEmpty(comboName) || string.IsNullOrEmpty(priceText))
            {
                MessageBox.Show("Please fill out all fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Combos SET ComboName = @ComboName, ComboPrice = @ComboPrice WHERE ComboSKU = @ComboSKU";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ComboName", comboName);
                        cmd.Parameters.AddWithValue("@ComboPrice", price);
                        cmd.Parameters.AddWithValue("@ComboSKU", comboSKU);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Combo updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the combo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
