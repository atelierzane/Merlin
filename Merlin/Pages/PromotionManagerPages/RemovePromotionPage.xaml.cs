using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class RemovePromotionPage : Page
    {
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();

        public RemovePromotionPage()
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
                                PromotionNameTextBlock.Text = reader["PromotionName"].ToString();
                                PromotionDetailsTextBlock.Text = $"Discount: {reader["PromotionDiscountValue"]}, " +
                                                                 $"Start Date: {Convert.ToDateTime(reader["PromotionStartDate"]):d}, " +
                                                                 $"End Date: {Convert.ToDateTime(reader["PromotionEndDate"]):d}";

                                PromotionInfoSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No promotion found with the given ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                PromotionInfoSection.Visibility = Visibility.Collapsed;
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

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            string promotionID = PromotionIDTextBox.Text.Trim();

            if (MessageBox.Show("Are you sure you want to delete this promotion?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var conn = new SqlConnection(databaseHelper.GetConnectionString()))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM Promotions WHERE PromotionID = @PromotionID";
                        using (var cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@PromotionID", promotionID);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Promotion removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                PromotionInfoSection.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MessageBox.Show("Failed to remove the promotion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
