using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class RemovePromotionBulkPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private ObservableCollection<Promotion> promotionCollection; // Bindable collection for the DataGrid

        public RemovePromotionBulkPage()
        {
            InitializeComponent();
            promotionCollection = new ObservableCollection<Promotion>();
            PromotionDataGrid.ItemsSource = promotionCollection; // Bind the DataGrid to the ObservableCollection
        }

        // Load promotions based on search criteria
        private void LoadPromotions(string promotionID, string promotionName, (decimal? minDiscount, decimal? maxDiscount)? discountRange, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT PromotionID, PromotionName, PromotionDiscountValue, PromotionStartDate, PromotionEndDate FROM Promotions WHERE 1=1";

                    if (!string.IsNullOrWhiteSpace(promotionID))
                    {
                        query += " AND PromotionID = @PromotionID";
                    }
                    if (!string.IsNullOrWhiteSpace(promotionName))
                    {
                        query += " AND PromotionName LIKE @PromotionName";
                    }
                    if (discountRange.HasValue)
                    {
                        if (discountRange.Value.minDiscount.HasValue)
                        {
                            query += " AND PromotionDiscountValue >= @MinDiscount";
                        }
                        if (discountRange.Value.maxDiscount.HasValue)
                        {
                            query += " AND PromotionDiscountValue < @MaxDiscount";
                        }
                    }
                    if (startDate.HasValue)
                    {
                        query += " AND PromotionStartDate >= @StartDate";
                    }
                    if (endDate.HasValue)
                    {
                        query += " AND PromotionEndDate <= @EndDate";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrWhiteSpace(promotionID))
                            cmd.Parameters.AddWithValue("@PromotionID", promotionID);
                        if (!string.IsNullOrWhiteSpace(promotionName))
                            cmd.Parameters.AddWithValue("@PromotionName", $"%{promotionName}%");
                        if (discountRange.HasValue)
                        {
                            if (discountRange.Value.minDiscount.HasValue)
                                cmd.Parameters.AddWithValue("@MinDiscount", discountRange.Value.minDiscount);
                            if (discountRange.Value.maxDiscount.HasValue)
                                cmd.Parameters.AddWithValue("@MaxDiscount", discountRange.Value.maxDiscount);
                        }
                        if (startDate.HasValue)
                            cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                        if (endDate.HasValue)
                            cmd.Parameters.AddWithValue("@EndDate", endDate.Value);

                        promotionCollection.Clear();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                promotionCollection.Add(new Promotion
                                {
                                    PromotionID = reader["PromotionID"].ToString(),
                                    PromotionName = reader["PromotionName"].ToString(),
                                    PromotionDiscountValue = Convert.ToDecimal(reader["PromotionDiscountValue"]),
                                    PromotionStartDate = Convert.ToDateTime(reader["PromotionStartDate"]),
                                    PromotionEndDate = Convert.ToDateTime(reader["PromotionEndDate"]),
                                    IsSelected = false
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

        // Search promotions based on filters
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string promotionID = PromotionIDTextBox.Text.Trim();
            string promotionName = PromotionNameTextBox.Text.Trim();
            decimal? minDiscount = null, maxDiscount = null;
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            if (decimal.TryParse(MinDiscountTextBox.Text, out decimal parsedMinDiscount))
                minDiscount = parsedMinDiscount;
            if (decimal.TryParse(MaxDiscountTextBox.Text, out decimal parsedMaxDiscount))
                maxDiscount = parsedMaxDiscount;

            LoadPromotions(promotionID, promotionName, (minDiscount, maxDiscount), startDate, endDate);
        }

        // Reset filters
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            PromotionIDTextBox.Clear();
            PromotionNameTextBox.Clear();
            MinDiscountTextBox.Clear();
            MaxDiscountTextBox.Clear();
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            promotionCollection.Clear();
        }

        // Select or deselect all promotions
        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = (sender as CheckBox).IsChecked == true;

            foreach (var promotion in promotionCollection)
            {
                promotion.IsSelected = isChecked;
            }

            PromotionDataGrid.Items.Refresh();
        }

        // Delete selected promotions
        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPromotions = new List<Promotion>();

            foreach (var promotion in promotionCollection)
            {
                if (promotion.IsSelected)
                {
                    selectedPromotions.Add(promotion);
                }
            }

            if (selectedPromotions.Count == 0)
            {
                MessageBox.Show("Please select at least one promotion to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete {selectedPromotions.Count} promotion(s)?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                foreach (var promotion in selectedPromotions)
                                {
                                    string deleteQuery = "DELETE FROM Promotions WHERE PromotionID = @PromotionID";
                                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@PromotionID", promotion.PromotionID);
                                        cmd.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                                MessageBox.Show($"{selectedPromotions.Count} promotion(s) deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                                SearchButton_Click(sender, e); // Refresh results
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Error deleting promotions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
