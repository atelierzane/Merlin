using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.DepartmentManagerPages
{
    public partial class ViewDepartmentsPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public ViewDepartmentsPage()
        {
            InitializeComponent();
            LoadAllCategories();
        }

        // Load all categories into the DataGrid
        private void LoadAllCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName FROM CategoryMap";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        List<Category> categories = new List<Category>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new Category
                                {
                                    CategoryID = reader["CategoryID"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                });
                            }
                        }

                        // Bind the retrieved categories to the DataGrid
                        CategoryDataGrid.ItemsSource = categories;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Search categories based on filter criteria
        private void SearchCategories(string categoryID, string categoryName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName FROM CategoryMap WHERE 1=1";

                    // Apply filters
                    if (!string.IsNullOrWhiteSpace(categoryID))
                    {
                        query += " AND CategoryID = @CategoryID";
                    }
                    if (!string.IsNullOrWhiteSpace(categoryName))
                    {
                        query += " AND CategoryName LIKE @CategoryName";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters
                        if (!string.IsNullOrWhiteSpace(categoryID))
                        {
                            cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                        }
                        if (!string.IsNullOrWhiteSpace(categoryName))
                        {
                            cmd.Parameters.AddWithValue("@CategoryName", $"%{categoryName}%");
                        }

                        List<Category> categories = new List<Category>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new Category
                                {
                                    CategoryID = reader["CategoryID"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                });
                            }
                        }

                        // Bind the retrieved categories to the DataGrid
                        CategoryDataGrid.ItemsSource = categories;
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
            string categoryID = CategoryIDTextBox.Text.Trim();
            string categoryName = CategoryNameTextBox.Text.Trim();

            SearchCategories(categoryID, categoryName);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryIDTextBox.Clear();
            CategoryNameTextBox.Clear();
            LoadAllCategories(); // Reset and reload all categories
        }

        private void DeleteAllDepartments_Click(object sender, RoutedEventArgs e)
        {
            // Confirm deletion
            var result = MessageBox.Show(
                "This will delete all departments. Are you sure you want to proceed?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();

                        // Delete all rows from the CategoryMap table
                        string deleteQuery = "DELETE FROM CategoryMap";
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("All departments have been deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Clear the DataGrid and reload data to reflect changes
                        LoadAllCategories();
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error deleting departments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
