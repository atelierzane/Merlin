using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.PromotionManagerPages
{
    public partial class AddPromotionPage : Page
    {
        public DatabaseHelper databaseHelper = new DatabaseHelper();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<PromotionTarget> PromotionTargets { get; set; } = new ObservableCollection<PromotionTarget>();

        public AddPromotionPage()
        {
            InitializeComponent();
            CategoryComboBox.ItemsSource = Categories; // Bind the ComboBox to Categories
            PromotionTargetsListBox.ItemsSource = PromotionTargets; // Bind the ListBox to PromotionTargets
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories(); // Load categories into ComboBox
        }

        private void PromotionTarget_Checked(object sender, RoutedEventArgs e)
        {
            if (SkuPanel != null && CategoryPanel != null)
            {
                SkuPanel.Visibility = rbTargetSKU.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
                CategoryPanel.Visibility = rbTargetCategory.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void AddSku_Click(object sender, RoutedEventArgs e)
        {
            string sku = SkuTextBox.Text.Trim();

            if (string.IsNullOrEmpty(sku))
            {
                MessageBox.Show("Please enter a valid SKU.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT ProductName FROM Catalog WHERE SKU = @SKU";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SKU", sku);
                        var productName = cmd.ExecuteScalar()?.ToString();

                        if (string.IsNullOrEmpty(productName))
                        {
                            MessageBox.Show("SKU not found in the catalog.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        PromotionTargets.Add(new PromotionTarget
                        {
                            SKU = sku,
                            CategoryID = null,
                            Description = $"SKU: {sku} - {productName}"
                        });

                        SkuTextBox.Clear();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryID = (string)CategoryComboBox.SelectedValue;
            string categoryName = ((Category)CategoryComboBox.SelectedItem)?.CategoryName;

            if (string.IsNullOrEmpty(categoryID) || string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Please select a valid category.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PromotionTargets.Add(new PromotionTarget
            {
                SKU = null,
                CategoryID = categoryID,
                Description = $"CategoryID: {categoryID} - {categoryName}"
            });

            // Reset the ComboBox selection
            CategoryComboBox.SelectedIndex = -1;
        }

        private void RemoveTarget_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var target = (PromotionTarget)button.DataContext;

            PromotionTargets.Remove(target);
        }
        private void SavePromotion_Click(object sender, RoutedEventArgs e)
        {
            // Ensure at least one target is added
            if (PromotionTargets.Count == 0)
            {
                MessageBox.Show("Please add at least one target (SKU or Category).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string promotionName = PromotionNameTextBox.Text.Trim();
            DateTime? startDate = PromotionStartDatePicker.SelectedDate;
            DateTime? endDate = PromotionEndDatePicker.SelectedDate;
            bool isPercentageDiscount = rbPercentageDiscount.IsChecked == true;
            bool isDollarDiscount = rbDollarDiscount.IsChecked == true;
            bool isGeneralPromotion = IsPromotionGeneralCheckBox.IsChecked == true;
            bool isLoyaltyPromotion = IsPromotionLoyaltyCheckBox.IsChecked == true;
            bool isCodeActivated = IsPromotionCodeActivatedCheckBox.IsChecked == true;

            // Validate required fields
            if (string.IsNullOrEmpty(promotionName) || !startDate.HasValue || !endDate.HasValue)
            {
                MessageBox.Show("Please fill in all required fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(DiscountValueTextBox.Text.Trim(), out decimal discountValue) || discountValue <= 0)
            {
                MessageBox.Show("Please enter a valid discount value.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();

                    string promotionID = GenerateUniquePromotionID();

                    string query = @"
                INSERT INTO Promotions (
                    PromotionID, PromotionName, PromotionStartDate, PromotionEndDate,
                    PromotionTargetSKU, PromotionTargetCategory, PromotionTargetLoyaltyTier,
                    PromotionTargetPriceLessThanOrEqualTo, PromotionTargetPriceGreaterThanOrEqualTo,
                    IsPromotionGeneral, IsPromotionLoyalty, IsPromotionCodeActivated,
                    PromotionActivationCode, IsPromotionPercentageValue, IsPromotionDollarValue, PromotionDiscountValue
                ) VALUES (
                    @PromotionID, @PromotionName, @PromotionStartDate, @PromotionEndDate,
                    @PromotionTargetSKU, @PromotionTargetCategory, @PromotionTargetLoyaltyTier,
                    @PromotionTargetPriceLessThanOrEqualTo, @PromotionTargetPriceGreaterThanOrEqualTo,
                    @IsPromotionGeneral, @IsPromotionLoyalty, @IsPromotionCodeActivated,
                    @PromotionActivationCode, @IsPromotionPercentageValue, @IsPromotionDollarValue, @PromotionDiscountValue
                )";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PromotionID", promotionID);
                        cmd.Parameters.AddWithValue("@PromotionName", promotionName);
                        cmd.Parameters.AddWithValue("@PromotionStartDate", startDate.Value);
                        cmd.Parameters.AddWithValue("@PromotionEndDate", endDate.Value);
                        cmd.Parameters.AddWithValue("@PromotionTargetSKU", GetTargetSKU());
                        cmd.Parameters.AddWithValue("@PromotionTargetCategory", GetTargetCategory());
                        cmd.Parameters.AddWithValue("@PromotionTargetLoyaltyTier", GetTargetLoyaltyTier());
                        cmd.Parameters.AddWithValue("@PromotionTargetPriceLessThanOrEqualTo", GetPriceFilterLessThan());
                        cmd.Parameters.AddWithValue("@PromotionTargetPriceGreaterThanOrEqualTo", GetPriceFilterGreaterThan());
                        cmd.Parameters.AddWithValue("@IsPromotionGeneral", isGeneralPromotion);
                        cmd.Parameters.AddWithValue("@IsPromotionLoyalty", isLoyaltyPromotion);
                        cmd.Parameters.AddWithValue("@IsPromotionCodeActivated", isCodeActivated);
                        cmd.Parameters.AddWithValue("@PromotionActivationCode", isCodeActivated ? ActivationCodeTextBox.Text.Trim() : DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsPromotionPercentageValue", isPercentageDiscount);
                        cmd.Parameters.AddWithValue("@IsPromotionDollarValue", isDollarDiscount);
                        cmd.Parameters.AddWithValue("@PromotionDiscountValue", discountValue);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Promotion saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private string GetTargetSKU()
        {
            var skuTarget = PromotionTargets.FirstOrDefault(target => !string.IsNullOrEmpty(target.SKU));
            return skuTarget?.SKU ?? DBNull.Value.ToString();
        }
        private string GetTargetCategory()
        {
            var categoryTarget = PromotionTargets.FirstOrDefault(target => !string.IsNullOrEmpty(target.CategoryID));
            return categoryTarget?.CategoryID ?? DBNull.Value.ToString();
        }

        private string GetTargetLoyaltyTier()
        {
            return LoyaltyTierComboBox.SelectedValue?.ToString() ?? DBNull.Value.ToString();
        }

        private string GenerateUniquePromotionID()
        {
            return Guid.NewGuid().ToString().Substring(0, 20); // Truncate to fit char(20)
        }
        private object GetPriceFilterLessThan()
        {
            // Attempt to parse the minimum price input
            if (decimal.TryParse(PriceMinTextBox.Text.Trim(), out decimal price))
            {
                return price; // Return the parsed decimal value
            }
            return DBNull.Value; // Return DBNull.Value if the input is invalid or empty
        }

        private object GetPriceFilterGreaterThan()
        {
            // Attempt to parse the maximum price input
            if (decimal.TryParse(PriceMaxTextBox.Text.Trim(), out decimal price))
            {
                return price; // Return the parsed decimal value
            }
            return DBNull.Value; // Return DBNull.Value if the input is invalid or empty
        }



        private void LoadCategories()
        {
            try
            {
                Categories.Clear();

                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName FROM CategoryMap";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Categories.Add(new Category
                                {
                                    CategoryID = reader["CategoryID"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
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

        private void IsPromotionCodeActivatedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ActivationCodePanel.Visibility = Visibility.Visible; // Show activation code input when checked
        }

        private void IsPromotionCodeActivatedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ActivationCodePanel.Visibility = Visibility.Collapsed; // Hide activation code input when unchecked
        }

        private void IsPromotionLoyaltyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LoyaltyTierPanel.Visibility = Visibility.Visible; // Show loyalty tier selection when checked
            LoadLoyaltyTiers(); // Load loyalty tiers into the ComboBox
        }

        private void IsPromotionLoyaltyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            LoyaltyTierPanel.Visibility = Visibility.Collapsed; // Hide loyalty tier selection when unchecked
        }

        private void LoadLoyaltyTiers()
        {
            try
            {
                LoyaltyTierComboBox.Items.Clear(); // Clear any existing items before loading

                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                SELECT LoyaltySKU, LoyaltyTier1Name, LoyaltyTier2Name, LoyaltyTier3Name
                FROM Loyalty";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                LoyaltyTierComboBox.Items.Add(new
                                {
                                    TierSKU = reader["LoyaltySKU"].ToString(),
                                    TierName = reader["LoyaltyTier1Name"].ToString() // Default to Tier1Name
                                });

                                // Optionally, include other tiers as separate items if needed
                                if (!string.IsNullOrEmpty(reader["LoyaltyTier2Name"].ToString()))
                                {
                                    LoyaltyTierComboBox.Items.Add(new
                                    {
                                        TierSKU = reader["LoyaltySKU"].ToString(),
                                        TierName = reader["LoyaltyTier2Name"].ToString()
                                    });
                                }

                                if (!string.IsNullOrEmpty(reader["LoyaltyTier3Name"].ToString()))
                                {
                                    LoyaltyTierComboBox.Items.Add(new
                                    {
                                        TierSKU = reader["LoyaltySKU"].ToString(),
                                        TierName = reader["LoyaltyTier3Name"].ToString()
                                    });
                                }
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



    }
}
