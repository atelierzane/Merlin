using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.CatalogManagerPages
{
    public partial class AddProductPage : Page
    {
        private int variantTypeCount = 0; // To limit to 3 variant types
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming you have a DatabaseHelper class
        private bool isTradeEligible = false;

        public AddProductPage()
        {
            InitializeComponent();
            LoadVendors();
            LoadCategories(); // Load categories when the page is initialized
        }

        // Method to load categories from the database into the ComboBox
        private void LoadCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName FROM CategoryMap"; // Adjust table and field names accordingly

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<Category> categories = new List<Category>();

                            while (reader.Read())
                            {
                                categories.Add(new Category
                                {
                                    CategoryID = reader["CategoryID"].ToString().Trim(),
                                    CategoryName = reader["CategoryName"].ToString().Trim()
                                });
                            }

                            CategoryIDComboBox.ItemsSource = categories; // Bind the ComboBox to the category list
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadVendors()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT VendorID, VendorName FROM Vendors ORDER BY VendorName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<Vendor> vendors = new List<Vendor>();

                            while (reader.Read())
                            {
                                vendors.Add(new Vendor
                                {
                                    VendorID = reader["VendorID"].ToString().Trim(),
                                    VendorName = reader["VendorName"].ToString().Trim()
                                });
                            }

                            VendorComboBox.ItemsSource = vendors;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading vendors: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CategoryIDComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string categoryID = CategoryIDComboBox.SelectedValue as string;

            if (!string.IsNullOrEmpty(categoryID))
            {
                CheckTradeEligibility(categoryID);
            }
        }

        private void CheckTradeEligibility(string categoryID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM CategoryTraits WHERE CategoryID = @CategoryID AND TraitName = 'TradeEligible'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                        int count = (int)cmd.ExecuteScalar();
                        isTradeEligible = count > 0;
                        TradeValuePanel.Visibility = isTradeEligible ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error checking trade eligibility: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handles the radio button check to show/hide the variant panel and UPC field
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (rbYes.IsChecked == true)
            {
                VariantPanel.Visibility = Visibility.Visible;
                btnAddVariantType.Visibility = Visibility.Visible;
                SingleUPCPanel.Visibility = Visibility.Collapsed; // Hide single UPC when variants are used
            }
            else
            {
                VariantPanel.Visibility = Visibility.Collapsed;
                btnAddVariantType.Visibility = Visibility.Collapsed;
                SingleUPCPanel.Visibility = Visibility.Visible; // Show single UPC when variants are not used
            }
        }

        // Handles mouse click before focus is set
        private void TextBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox && !textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus(); // This will trigger GotKeyboardFocus next
            }
        }

        // Selects all text if it matches the placeholder
        private void TextBox_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string defaultText = textBox.Tag as string;
                if (!string.IsNullOrEmpty(defaultText) && textBox.Text == defaultText)
                {
                    textBox.SelectAll();
                }
            }
        }

        // Restores placeholder if user leaves the box empty
        private void RestorePlaceholderIfEmpty(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string defaultText = textBox.Tag as string;
                if (!string.IsNullOrEmpty(defaultText) && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = defaultText;
                }
            }
        }

        // Adds a new variant type (e.g., 'Color', 'Size') with a maximum of 3 types
        private void AddVariantType_Click(object sender, RoutedEventArgs e)
        {
            if (variantTypeCount < 3) // Limit to 3 variant types
            {
                var variantTypePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 0, 0, 15)
                };

                var variantTypeTextBox = new TextBox
                {
                    Width = 250,
                    Margin = new Thickness(0, 0, 10, 0),
                    Text = "Variant Name",
                    Tag = "Variant Name",
                    Style = (Style)FindResource("LightMinimalistSearchBoxStyle")
                };
                variantTypeTextBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
                variantTypeTextBox.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
                variantTypeTextBox.LostFocus += RestorePlaceholderIfEmpty;

                var removeTypeButton = new Button
                {
                    Content = "-",
                    Width = 35,
                    Height = 35,
                    Margin = new Thickness(0, 0, 0, 0),
                    Style = (Style)FindResource("LightMinimalistButtonStyle_Short")
                };
                removeTypeButton.Click += RemoveVariantType_Click;

                var headerPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                headerPanel.Children.Add(variantTypeTextBox);
                headerPanel.Children.Add(removeTypeButton);

                var variantValuesStack = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Name = "VariantValuesStack",
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var variantValuePanel = CreateVariantValuePanel(variantValuesStack);

                variantTypePanel.Children.Add(headerPanel);
                variantTypePanel.Children.Add(variantValuesStack);
                variantValuesStack.Children.Add(variantValuePanel);

                VariantTypeList.Items.Add(variantTypePanel);
                variantTypeCount++;
            }
            else
            {
                MessageBox.Show("You can only add up to 3 variant types.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Creates a variant value panel (horizontal StackPanel) and applies XAML styles to its elements
        private StackPanel CreateVariantValuePanel(StackPanel parentStack)
        {
            var variantValuePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var variantValueTextBox = new TextBox
            {
                Width = 150,
                Margin = new Thickness(0, 0, 10, 0),
                Text = "Variant Value",
                Tag = "Variant Value",
                Style = (Style)FindResource("LightMinimalistSearchBoxStyle")
            };
            variantValueTextBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            variantValueTextBox.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
            variantValueTextBox.LostFocus += RestorePlaceholderIfEmpty;

            var upcTextBox = new TextBox
            {
                Width = 200,
                Margin = new Thickness(0, 0, 10, 0),
                Text = "UPC",
                Tag = "UPC",
                Style = (Style)FindResource("LightMinimalistSearchBoxStyle")
            };
            upcTextBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            upcTextBox.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
            upcTextBox.LostFocus += RestorePlaceholderIfEmpty;

            var addValueButton = new Button
            {
                Content = "+",
                Width = 35,
                Height = 35,
                Margin = new Thickness(0, 0, 5, 0),
                Style = (Style)FindResource("LightMinimalistButtonStyle_Short"),
                Tag = parentStack
            };
            addValueButton.Click += AddVariantValue_Click;

            var removeValueButton = new Button
            {
                Content = "-",
                Width = 35,
                Height = 35,
                Style = (Style)FindResource("LightMinimalistButtonStyle_Short"),
                Tag = variantValuePanel
            };
            removeValueButton.Click += RemoveVariantValue_Click;

            variantValuePanel.Children.Add(variantValueTextBox);
            variantValuePanel.Children.Add(upcTextBox);
            variantValuePanel.Children.Add(addValueButton);
            variantValuePanel.Children.Add(removeValueButton);

            return variantValuePanel;
        }


        // Adds a new variant value (e.g., 'Red', 'Blue') with a UPC field to the selected variant type
        private void AddVariantValue_Click(object sender, RoutedEventArgs e)
        {
            var addButton = sender as Button;
            var parentStack = addButton?.Tag as StackPanel;

            if (parentStack != null)
            {
                var newVariantValuePanel = CreateVariantValuePanel(parentStack);
                parentStack.Children.Add(newVariantValuePanel);
            }
        }

        // Removes a variant type (entire variant group)
        private void RemoveVariantType_Click(object sender, RoutedEventArgs e)
        {
            var removeButton = sender as Button;
            var parentPanel = removeButton?.Parent as StackPanel;

            if (parentPanel != null && parentPanel.Parent is StackPanel)
            {
                var variantTypePanel = parentPanel.Parent as StackPanel;
                VariantTypeList.Items.Remove(variantTypePanel); // Remove the whole variant panel
                variantTypeCount--; // Decrement the count of variant types
            }
        }

        // Removes a single variant value from the variant type
        private void RemoveVariantValue_Click(object sender, RoutedEventArgs e)
        {
            var removeButton = sender as Button;
            var parentPanel = removeButton?.Tag as StackPanel;

            if (parentPanel != null && parentPanel.Parent is StackPanel)
            {
                var grandparentPanel = parentPanel.Parent as StackPanel;
                grandparentPanel.Children.Remove(parentPanel); // Remove the specific variant value panel
            }
        }

        private string GenerateSKU(string categoryID, string productName, string variant = "", int attemptNumber = 0, bool isBaseSKU = false)
        {
            if (string.IsNullOrEmpty(categoryID) || productName.Length < 2)
            {
                throw new ArgumentException("Invalid CategoryID or ProductName for SKU generation.");
            }

            // Ensure CategoryID is always 3 digits
            string categoryPart = categoryID.PadLeft(3, '0');

            // Generate a random 4-digit number for the middle part
            Random random = new Random();
            string middlePart = random.Next(1000, 9999).ToString("0000");

            string finalPart;

            if (isBaseSKU)
            {
                // If it's a base SKU, the last three characters are always "000"
                finalPart = "000";
                // Truncate the middlePart if necessary to ensure SKU is exactly 9 characters
                middlePart = middlePart.Substring(0, middlePart.Length - 1);
            }
            else if (!string.IsNullOrEmpty(variant))
            {
                // Generate a hash from the variant string to ensure uniqueness
                finalPart = (Math.Abs(variant.GetHashCode()) + attemptNumber).ToString("000").Substring(0, 3);
            }
            else
            {
                // Generate random digits for non-base and non-variant products
                finalPart = random.Next(100, 999).ToString("000");
            }

            // Concatenate the parts to form the final SKU
            string sku = categoryPart + middlePart + finalPart;

            // Ensure the SKU is exactly 9 characters long
            return sku.Length > 9 ? sku.Substring(0, 9) : sku;
        }

        private string GenerateUniqueSKU(SqlConnection conn, string categoryID, string productName, string variant, bool isBaseSKU = false)
        {
            const int retryLimit = 100; // Increased retry limit
            int attempts = 0;
            string sku = null;

            while (attempts < retryLimit)
            {
                sku = GenerateSKU(categoryID, productName, variant, attempts, isBaseSKU);

                // Check if the SKU already exists in the database
                using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Catalog WHERE SKU = @SKU", conn))
                {
                    checkCmd.Parameters.AddWithValue("@SKU", sku);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        return sku; // SKU is unique
                    }
                }

                attempts++;
            }

            // If we fail to generate a unique SKU after retrying, log or throw an error
            throw new Exception("Failed to generate a unique SKU after " + retryLimit + " attempts.");
        }




        private Dictionary<string, List<string>> GenerateVariantCombinations(
            SqlCommand cmd,
            SqlConnection conn,
            string categoryID,
            string productName,
            decimal price,
            decimal tradeValue,
            decimal cost,
            string vendorID,
            List<string> variantNames,
            List<List<string>> variantValuesList,
            string baseSKU)
        {
            var combinations = GetCombinations(variantValuesList);
            var aggregatedProperties = new Dictionary<string, List<string>>
    {
        { "Variant1Properties", new List<string>() },
        { "Variant2Properties", new List<string>() },
        { "Variant3Properties", new List<string>() }
    };

            foreach (var combination in combinations)
            {
                string variantCombination = string.Join(", ", combination);
                string sku = GenerateUniqueSKU(conn, categoryID, productName, variantCombination);

                if (sku == null)
                {
                    MessageBox.Show("Failed to generate a unique SKU for a variant after 50 attempts.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                string variantProductName = productName + " (" + string.Join(") (", combination) + ")";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SKU", sku);
                cmd.Parameters.AddWithValue("@ProductName", variantProductName);
                cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@TradeValue", tradeValue);
                cmd.Parameters.AddWithValue("@Cost", cost);
                cmd.Parameters.AddWithValue("@VendorID", vendorID);
                cmd.Parameters.AddWithValue("@UPC", DBNull.Value);
                cmd.Parameters.AddWithValue("@IsBaseSKU", 0);
                cmd.Parameters.AddWithValue("@IsVariantSKU", 1);
                cmd.Parameters.AddWithValue("@VariantAssignedToBaseSKU", baseSKU);
                cmd.Parameters.AddWithValue("@Variant1Name", variantNames.Count > 0 ? variantNames[0] : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Variant1Properties", combination.Count > 0 ? combination[0] : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Variant2Name", variantNames.Count > 1 ? variantNames[1] : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Variant2Properties", combination.Count > 1 ? combination[1] : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Variant3Name", variantNames.Count > 2 ? variantNames[2] : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Variant3Properties", combination.Count > 2 ? combination[2] : (object)DBNull.Value);

                cmd.ExecuteNonQuery();

                // Aggregate unique properties
                if (combination.Count > 0 && !aggregatedProperties["Variant1Properties"].Contains(combination[0]))
                    aggregatedProperties["Variant1Properties"].Add(combination[0]);
                if (combination.Count > 1 && !aggregatedProperties["Variant2Properties"].Contains(combination[1]))
                    aggregatedProperties["Variant2Properties"].Add(combination[1]);
                if (combination.Count > 2 && !aggregatedProperties["Variant3Properties"].Contains(combination[2]))
                    aggregatedProperties["Variant3Properties"].Add(combination[2]);
            }

            return aggregatedProperties;
        }




        // Recursive method to generate all combinations of variant values
        private List<List<string>> GetCombinations(List<List<string>> lists)
        {
            List<List<string>> result = new List<List<string>>();

            // Base case: If there are no more lists, return a list with an empty list
            if (lists.Count == 0)
            {
                result.Add(new List<string>());
                return result;
            }

            // Get combinations of the remaining lists
            var remainingCombinations = GetCombinations(lists.GetRange(1, lists.Count - 1));

            // Prepend each item from the first list to each combination of the remaining lists
            foreach (var item in lists[0])
            {
                foreach (var combination in remainingCombinations)
                {
                    var newCombination = new List<string> { item };
                    newCombination.AddRange(combination);
                    result.Add(newCombination);
                }
            }

            return result;
        }

        // Save product and variants
        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            string categoryID = (string)CategoryIDComboBox.SelectedValue;
            string productName = ProductNameTextBox.Text.Trim();
            string priceText = ProductPriceTextBox.Text.Trim();
            decimal price;
            decimal tradeValue = 0;
            string costText = CostTextBox.Text.Trim();

            decimal? cost = null;
            if (decimal.TryParse(costText, out decimal parsedCost) && parsedCost >= 0)
                cost = parsedCost;

            string selectedVendorID = VendorComboBox.SelectedValue as string;
            if (string.IsNullOrWhiteSpace(selectedVendorID))
                selectedVendorID = null;

            if (!decimal.TryParse(priceText, out price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isTradeEligible)
            {
                if (!decimal.TryParse(TradeValueTextBox.Text.Trim(), out tradeValue) || tradeValue < 0)
                {
                    MessageBox.Show("Please enter a valid trade value.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                tradeValue = 0;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string getLocationQuery = "SELECT LocationID FROM Location";
                    List<string> locationIDs = new List<string>();

                    using (SqlCommand getLocationCmd = new SqlCommand(getLocationQuery, conn))
                    {
                        using (SqlDataReader reader = getLocationCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                locationIDs.Add(reader["LocationID"].ToString());
                            }
                        }
                    }

                    string insertCatalogQuery = @"
                INSERT INTO Catalog 
                (SKU, ProductName, CategoryID, Price, TradeValue, Cost, VendorID, UPC, Variant1Name, Variant1Properties, Variant2Name, Variant2Properties, Variant3Name, Variant3Properties, IsBaseSKU, IsVariantSKU, VariantAssignedToBaseSKU, IsInventoryTracked) 
                VALUES 
                (@SKU, @ProductName, @CategoryID, @Price, @TradeValue, @Cost, @VendorID, @UPC, @Variant1Name, @Variant1Properties, @Variant2Name, @Variant2Properties, @Variant3Name, @Variant3Properties, @IsBaseSKU, @IsVariantSKU, @VariantAssignedToBaseSKU, @IsInventoryTracked)";

                    using (SqlCommand cmd = new SqlCommand(insertCatalogQuery, conn))
                    {
                        if (rbYes.IsChecked == true)
                        {
                            List<string> variantNames = new List<string>();
                            List<List<string>> variantValuesList = new List<List<string>>();

                            foreach (StackPanel variantTypePanel in VariantTypeList.Items)
                            {
                                TextBox variantTypeTextBox = (TextBox)((StackPanel)variantTypePanel.Children[0]).Children[0];
                                StackPanel variantValuesStack = (StackPanel)variantTypePanel.Children[1];

                                string variantName = variantTypeTextBox.Text.Trim();
                                variantNames.Add(variantName);

                                List<string> variantValues = new List<string>();
                                foreach (StackPanel variantValuePanel in variantValuesStack.Children)
                                {
                                    TextBox variantValueTextBox = (TextBox)variantValuePanel.Children[0];
                                    variantValues.Add(variantValueTextBox.Text.Trim());
                                }

                                variantValuesList.Add(variantValues);
                            }

                            string baseSku = GenerateUniqueSKU(conn, categoryID, productName, null, true);

                            if (baseSku == null)
                            {
                                MessageBox.Show("Failed to generate a unique SKU for the base product after 50 attempts.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@SKU", baseSku);
                            cmd.Parameters.AddWithValue("@ProductName", productName);
                            cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                            cmd.Parameters.AddWithValue("@Price", price);
                            cmd.Parameters.AddWithValue("@TradeValue", tradeValue);
                            cmd.Parameters.AddWithValue("@UPC", DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsBaseSKU", 1);
                            cmd.Parameters.AddWithValue("@IsVariantSKU", 0);
                            cmd.Parameters.AddWithValue("@VariantAssignedToBaseSKU", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant1Name", variantNames.Count > 0 ? variantNames[0] : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant1Properties", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant2Name", variantNames.Count > 1 ? variantNames[1] : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant2Properties", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant3Name", variantNames.Count > 2 ? variantNames[2] : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant3Properties", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Cost", (object?)cost ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@VendorID", (object?)selectedVendorID ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsInventoryTracked", TrackInventoryCheckBox.IsChecked == true);

                            cmd.ExecuteNonQuery();

                            var aggregatedProperties = GenerateVariantCombinations(
                                cmd, conn, categoryID, productName, price, tradeValue,
                                cost ?? 0, selectedVendorID ?? "",
                                variantNames, variantValuesList, baseSku);

                            string updateBaseSkuQuery = @"
                        UPDATE Catalog
                        SET Variant1Properties = @Variant1Properties,
                            Variant2Properties = @Variant2Properties,
                            Variant3Properties = @Variant3Properties
                        WHERE SKU = @BaseSKU";

                            using (SqlCommand updateCmd = new SqlCommand(updateBaseSkuQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@Variant1Properties", string.Join(", ", aggregatedProperties["Variant1Properties"]));
                                updateCmd.Parameters.AddWithValue("@Variant2Properties", string.Join(", ", aggregatedProperties["Variant2Properties"]));
                                updateCmd.Parameters.AddWithValue("@Variant3Properties", string.Join(", ", aggregatedProperties["Variant3Properties"]));
                                updateCmd.Parameters.AddWithValue("@BaseSKU", baseSku);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string sku = GenerateUniqueSKU(conn, categoryID, productName, null);
                            string upc = SingleUPCTextBox.Text.Trim();

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@SKU", sku);
                            cmd.Parameters.AddWithValue("@ProductName", productName);
                            cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                            cmd.Parameters.AddWithValue("@Price", price);
                            cmd.Parameters.AddWithValue("@TradeValue", tradeValue);
                            cmd.Parameters.AddWithValue("@UPC", upc);
                            cmd.Parameters.AddWithValue("@Variant1Name", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant1Properties", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant2Name", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant2Properties", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant3Name", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variant3Properties", DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsBaseSKU", 0);
                            cmd.Parameters.AddWithValue("@IsVariantSKU", 0);
                            cmd.Parameters.AddWithValue("@VariantAssignedToBaseSKU", DBNull.Value);
                            cmd.Parameters.AddWithValue("@Cost", (object?)cost ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@VendorID", (object?)selectedVendorID ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@IsInventoryTracked", TrackInventoryCheckBox.IsChecked == true);

                            cmd.ExecuteNonQuery();

                            string sisterCategoryID = "9" + categoryID.Substring(1);

                            string checkSisterCategoryQuery = "SELECT CategoryName FROM CategoryMap WHERE CategoryID = @SisterCategoryID AND CategoryName LIKE '%(Used)%'";
                            using (SqlCommand checkSisterCmd = new SqlCommand(checkSisterCategoryQuery, conn))
                            {
                                checkSisterCmd.Parameters.AddWithValue("@SisterCategoryID", sisterCategoryID);
                                var result = checkSisterCmd.ExecuteScalar();

                                if (result != null)
                                {
                                    string usedSku = GenerateUniqueSKU(conn, sisterCategoryID, productName + " (Used)", null);

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@SKU", usedSku);
                                    cmd.Parameters.AddWithValue("@ProductName", productName + " (Used)");
                                    cmd.Parameters.AddWithValue("@CategoryID", sisterCategoryID);
                                    cmd.Parameters.AddWithValue("@Price", price);
                                    cmd.Parameters.AddWithValue("@TradeValue", tradeValue);
                                    cmd.Parameters.AddWithValue("@UPC", upc);
                                    cmd.Parameters.AddWithValue("@Variant1Name", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Variant1Properties", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Variant2Name", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Variant2Properties", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Variant3Name", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Variant3Properties", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@IsBaseSKU", 0);
                                    cmd.Parameters.AddWithValue("@IsVariantSKU", 0);
                                    cmd.Parameters.AddWithValue("@VariantAssignedToBaseSKU", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Cost", (object?)cost ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@VendorID", (object?)selectedVendorID ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@IsInventoryTracked", TrackInventoryCheckBox.IsChecked == true);

                                    cmd.ExecuteNonQuery();

                                    AddInventoryRecordsForLocations(conn, usedSku, sisterCategoryID, locationIDs);

                                    if (AddToQuickSelectCheckBox.IsChecked == true)
                                    {
                                        AddToQuickSelect(conn, usedSku, locationIDs);
                                    }
                                }
                            }

                            AddInventoryRecordsForLocations(conn, sku, categoryID, locationIDs);

                            if (AddToQuickSelectCheckBox.IsChecked == true)
                            {
                                AddToQuickSelect(conn, sku, locationIDs);
                            }
                        }
                    }

                    MessageBox.Show("Product saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddToQuickSelect(SqlConnection conn, string sku, List<string> locationIDs)
        {
            string insertQuickSelectQuery = "INSERT INTO LocationQuickSelect (LocationID, SKU) VALUES (@LocationID, @SKU)";

            using (SqlCommand cmd = new SqlCommand(insertQuickSelectQuery, conn))
            {
                foreach (string locationID in locationIDs)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                    cmd.Parameters.AddWithValue("@SKU", sku);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AddInventoryRecordsForLocations(SqlConnection conn, string sku, string categoryID, List<string> locationIDs)
        {
            // Update the insert query to include CategoryID field in the Inventory table
            string insertInventoryQuery = "INSERT INTO Inventory (SKU, CategoryID, LocationID, QuantityOnHandSellable, QuantityOnHandDefective) " +
                                          "VALUES (@SKU, @CategoryID, @LocationID, @QuantityOnHandSellable, @QuantityOnHandDefective)";

            using (SqlCommand cmd = new SqlCommand(insertInventoryQuery, conn))
            {
                foreach (string locationID in locationIDs)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@SKU", sku);
                    cmd.Parameters.AddWithValue("@CategoryID", categoryID); // Ensure CategoryID is included
                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                    cmd.Parameters.AddWithValue("@QuantityOnHandSellable", 0); // Set sellable quantity to 0
                    cmd.Parameters.AddWithValue("@QuantityOnHandDefective", 0); // Set defective quantity to 0
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

