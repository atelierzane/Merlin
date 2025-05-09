using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;
using static MerlinAdministrator.Models.CategoryTrait;

namespace MerlinAdministrator.Pages.DepartmentManagerPages
{
    public partial class AddDepartmentPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private ObservableCollection<CategoryTrait> categoryTraits = new ObservableCollection<CategoryTrait>();

        public AddDepartmentPage()
        {
            InitializeComponent();
            TraitListView.ItemsSource = categoryTraits; // Bind traits list to ListView
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

        // Handles saving the department/category and its traits to the database
        private void SaveDepartment_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = CategoryNameTextBox.Text.Trim();
            string categoryID = CategoryIDTextBox.Text.Trim();
            bool createUsedDepartment = UsedDepartmentCheckBox.IsChecked == true;

            if (categoryID.Length != 3 || !int.TryParse(categoryID, out _))
            {
                MessageBox.Show("Category ID must be a 3-digit numerical value.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Category Name cannot be empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Check if CategoryID already exists
                    if (CategoryExists(conn, categoryID))
                    {
                        MessageBox.Show($"A category with ID '{categoryID}' already exists.", "Duplicate Category ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // If creating used department, also check its
                    string usedCategoryID = null;
                    if (createUsedDepartment)
                    {
                        usedCategoryID = "9" + categoryID.Substring(1);
                        if (CategoryExists(conn, usedCategoryID))
                        {
                            MessageBox.Show($"A used category with ID '{usedCategoryID}' already exists.", "Duplicate Used Category ID", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Insert main category
                    InsertCategory(conn, categoryID, categoryName);
                    InsertTraits(conn, categoryID);

                    // Optionally insert used version
                    if (createUsedDepartment)
                    {
                        string usedCategoryName = categoryName + " (Used)";
                        InsertCategory(conn, usedCategoryID, usedCategoryName);
                        InsertTraits(conn, usedCategoryID);
                    }

                    MessageBox.Show("Category and traits added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Helper method to insert category
        private void InsertCategory(SqlConnection conn, string categoryID, string categoryName)
        {
            string insertCategoryQuery = "INSERT INTO CategoryMap (CategoryID, CategoryName) VALUES (@CategoryID, @CategoryName)";
            using (SqlCommand cmd = new SqlCommand(insertCategoryQuery, conn))
            {
                cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                cmd.ExecuteNonQuery();
            }
        }

        // Helper method to insert traits
        private void InsertTraits(SqlConnection conn, string categoryID)
        {
            string insertTraitQuery = "INSERT INTO CategoryTraits (CategoryID, TraitName, IsRequired) VALUES (@CategoryID, @TraitName, @IsRequired)";
            foreach (var trait in categoryTraits)
            {
                using (SqlCommand traitCmd = new SqlCommand(insertTraitQuery, conn))
                {
                    traitCmd.Parameters.AddWithValue("@CategoryID", categoryID);
                    traitCmd.Parameters.AddWithValue("@TraitName", trait.Trait.ToString());  // Store the trait as a string
                    traitCmd.Parameters.AddWithValue("@IsRequired", trait.IsRequired);
                    traitCmd.ExecuteNonQuery();
                }
            }
        }

        private bool CategoryExists(SqlConnection conn, string categoryID)
        {
            string query = "SELECT COUNT(*) FROM CategoryMap WHERE CategoryID = @CategoryID";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }


    }
}
