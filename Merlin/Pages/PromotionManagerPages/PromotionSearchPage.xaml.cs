using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class PromotionSearchPage : Page
    {
        public DatabaseHelper dbHelper = new DatabaseHelper();

        public PromotionSearchPage()
        {
            InitializeComponent();
        }

        private void LoadPromotions(string promotionName, DateTime? startDate, DateTime? endDate, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT PromotionID, PromotionName, PromotionStartDate, PromotionEndDate, 
                                            PromotionDiscountValue, 
                                            CASE 
                                                WHEN IsPromotionPercentageValue = 1 THEN 'Percentage'
                                                WHEN IsPromotionDollarValue = 1 THEN 'Dollar'
                                                ELSE 'None'
                                            END AS PromotionType
                                     FROM Promotions
                                     WHERE 1=1";

                    // Apply filters
                    if (!string.IsNullOrWhiteSpace(promotionName))
                    {
                        query += " AND PromotionName LIKE @PromotionName";
                    }
                    if (startDate.HasValue)
                    {
                        query += " AND PromotionStartDate >= @StartDate";
                    }
                    if (endDate.HasValue)
                    {
                        query += " AND PromotionEndDate <= @EndDate";
                    }

                    // Apply status filter
                    if (status == "Active")
                    {
                        query += " AND PromotionStartDate <= GETDATE() AND PromotionEndDate >= GETDATE()";
                    }
                    else if (status == "Inactive")
                    {
                        query += " AND PromotionEndDate < GETDATE()";
                    }
                    else if (status == "Upcoming")
                    {
                        query += " AND PromotionStartDate > GETDATE()";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters
                        if (!string.IsNullOrWhiteSpace(promotionName))
                            cmd.Parameters.AddWithValue("@PromotionName", $"%{promotionName}%");
                        if (startDate.HasValue)
                            cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                        if (endDate.HasValue)
                            cmd.Parameters.AddWithValue("@EndDate", endDate.Value);

                        // Execute the query
                        List<Promotion> promotions = new List<Promotion>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                promotions.Add(new Promotion
                                {
                                    PromotionID = reader["PromotionID"].ToString(),
                                    PromotionName = reader["PromotionName"].ToString(),
                                    PromotionStartDate = Convert.ToDateTime(reader["PromotionStartDate"]),
                                    PromotionEndDate = Convert.ToDateTime(reader["PromotionEndDate"]),
                                    PromotionDiscountValue = reader["PromotionDiscountValue"] != DBNull.Value
                                        ? Convert.ToDecimal(reader["PromotionDiscountValue"])
                                        : (decimal?)null,
                                    PromotionType = reader["PromotionType"].ToString()
                                });
                            }
                        }

                        // Bind the retrieved promotions to the DataGrid
                        PromotionDataGrid.ItemsSource = promotions;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string promotionName = PromotionNameTextBox.Text.Trim();
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            // Get selected status
            string status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();

            // Load promotions with filters
            LoadPromotions(promotionName, startDate, endDate, status);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            PromotionNameTextBox.Clear();
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            StatusComboBox.SelectedIndex = 0; // Reset to "All"

            PromotionDataGrid.ItemsSource = null; // Clear the grid
        }
    }
}
