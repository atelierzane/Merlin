using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class RemoveComboPage : Page
    {
        public DatabaseHelper databaseHelper = new DatabaseHelper();

        public RemoveComboPage()
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
                                ComboNameTextBlock.Text = reader["ComboName"].ToString();
                                PriceTextBlock.Text = reader["ComboPrice"].ToString();

                                ComboInfoSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No combo found with the given SKU.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                ComboInfoSection.Visibility = Visibility.Collapsed;
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

        // Remove the combo
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            string comboSKU = SkuTextBox.Text.Trim();

            if (MessageBox.Show("Are you sure you want to delete this combo?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM Combos WHERE ComboSKU = @ComboSKU";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@ComboSKU", comboSKU);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Combo removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                ComboInfoSection.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MessageBox.Show("Failed to remove the combo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
