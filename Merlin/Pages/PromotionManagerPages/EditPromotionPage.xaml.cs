using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class EditPromotionPage : Page
    {
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();

        public EditPromotionPage()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string promotionID = PromotionIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(promotionID))
            {
                MessageBox.Show("Please enter a Promotion ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT PromotionName, PromotionDiscountValue, PromotionStartDate, PromotionEndDate FROM Promotions WHERE PromotionID = @PromotionID";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PromotionID", promotionID);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                PromotionNameTextBox.Text = reader["PromotionName"].ToString();
                                DiscountValueTextBox.Text = reader["PromotionDiscountValue"].ToString();
                                StartDatePicker.SelectedDate = Convert.ToDateTime(reader["PromotionStartDate"]);
                                EndDatePicker.SelectedDate = Convert.ToDateTime(reader["PromotionEndDate"]);

                                PromotionEditSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No promotion found with the given ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                PromotionEditSection.Visibility = Visibility.Collapsed;
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

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string promotionID = PromotionIDTextBox.Text.Trim();
            string promotionName = PromotionNameTextBox.Text.Trim();
            string discountValueText = DiscountValueTextBox.Text.Trim();

            if (string.IsNullOrEmpty(promotionName) || string.IsNullOrEmpty(discountValueText) || !StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please fill out all fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(discountValueText, out decimal discountValue) || discountValue <= 0)
            {
                MessageBox.Show("Please enter a valid discount value.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string updateQuery = @"UPDATE Promotions 
                                           SET PromotionName = @PromotionName, PromotionDiscountValue = @DiscountValue, 
                                               PromotionStartDate = @StartDate, PromotionEndDate = @EndDate 
                                           WHERE PromotionID = @PromotionID";

                    using (var cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PromotionName", promotionName);
                        cmd.Parameters.AddWithValue("@DiscountValue", discountValue);
                        cmd.Parameters.AddWithValue("@StartDate", StartDatePicker.SelectedDate.Value);
                        cmd.Parameters.AddWithValue("@EndDate", EndDatePicker.SelectedDate.Value);
                        cmd.Parameters.AddWithValue("@PromotionID", promotionID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Promotion updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the promotion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
