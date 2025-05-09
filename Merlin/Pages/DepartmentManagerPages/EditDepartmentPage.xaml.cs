using MerlinAdministrator.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.DepartmentManagerPages
{
    public partial class EditDepartmentPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming you have a DatabaseHelper class
        private ObservableCollection<CategoryTrait> categoryTraits = new ObservableCollection<CategoryTrait>();

        public EditDepartmentPage()
        {
            InitializeComponent();
            TraitListView.ItemsSource = categoryTraits; // Bind the traits list to the ListView
        }

        // Search for the category by Category ID and load its details and traits
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string categoryID = CategoryIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(categoryID))
            {
                MessageBox.Show("Please enter a Category ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CategoryName FROM CategoryMap WHERE CategoryID = @CategoryID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                // Populate the fields with the category data
                                CategoryNameTextBox.Text = reader["CategoryName"].ToString();

                                // Load the associated traits
                                LoadCategoryTraits(categoryID);

                                // Show the edit fields
                                CategoryEditSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No category found with the given Category ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                CategoryEditSection.Visibility = Visibility.Collapsed;
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

        // Load category traits from the database
        private void LoadCategoryTraits(string categoryID)
        {
            categoryTraits.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT TraitName, IsRequired FROM CategoryTraits WHERE CategoryID = @CategoryID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var traitName = reader["TraitName"].ToString();
                                var isRequired = (bool)reader["IsRequired"];

                                // Convert the stored string back to the PredefinedTrait enum
                                if (Enum.TryParse(traitName, out PredefinedTrait trait))
                                {
                                    categoryTraits.Add(new CategoryTrait { Trait = trait, IsRequired = isRequired });
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading traits: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Adds a new trait to the list
        private void AddTrait_Click(object sender, RoutedEventArgs e)
        {
            categoryTraits.Add(new CategoryTrait { Trait = PredefinedTrait.Serialized, IsRequired = false });
        }

        // Removes a selected trait from the list
        private void RemoveTrait_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button removeButton && removeButton.DataContext is CategoryTrait trait)
            {
                categoryTraits.Remove(trait);
            }
        }

        // Update the category and its traits in the database
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string categoryID = CategoryIDTextBox.Text.Trim();
            string categoryName = CategoryNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Please fill out the category name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string updateQuery = "UPDATE CategoryMap SET CategoryName = @CategoryName WHERE CategoryID = @CategoryID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                        cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                        cmd.ExecuteNonQuery();
                    }

                    // Clear existing traits in the database
                    string deleteTraitQuery = "DELETE FROM CategoryTraits WHERE CategoryID = @CategoryID";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteTraitQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@CategoryID", categoryID);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Insert updated traits
                    string insertTraitQuery = "INSERT INTO CategoryTraits (CategoryID, TraitName, IsRequired) VALUES (@CategoryID, @TraitName, @IsRequired)";
                    foreach (var trait in categoryTraits)
                    {
                        using (SqlCommand traitCmd = new SqlCommand(insertTraitQuery, conn))
                        {
                            traitCmd.Parameters.AddWithValue("@CategoryID", categoryID);
                            traitCmd.Parameters.AddWithValue("@TraitName", trait.Trait.ToString());
                            traitCmd.Parameters.AddWithValue("@IsRequired", trait.IsRequired);
                            traitCmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Category and traits updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
