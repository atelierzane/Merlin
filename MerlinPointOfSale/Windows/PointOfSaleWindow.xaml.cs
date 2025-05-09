using MerlinPointOfSale.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Linq;
using MerlinPointOfSale.Repositories;
using System.Data.SqlClient;
using System.Windows.Data;
using MerlinPointOfSale.Pages;
using MerlinPointOfSale.Windows.DialogWindows;
using MerlinPointOfSale.Pages;
using MerlinPointOfSale.Converters;
using MerlinPointOfSale.Helpers;

namespace MerlinPointOfSale
{
    public partial class PointOfSaleWindow : Window
    {
        private TransactionSummary transactionSummary;
        private DatabaseHelper databaseHelper = new DatabaseHelper();


        private ProductRepository productRepository;

        private List<Promotion> activePromotions; // To store active promotions

        public ObservableCollection<Customer> Customer {get; private set;}
        public ObservableCollection<InventoryItem> SaleItems { get; private set; }
        public ObservableCollection<InventoryItem> ReturnItems { get; private set; }
        public ObservableCollection<TradeItem> TradeItems { get; private set; }
        public ObservableCollection<SummaryItem> SummaryItems { get; private set; }

        private decimal taxRate;
        private decimal subtotal;
        private decimal returnsSubtotal;
        private decimal taxes;
        private decimal total;

        public string employeeID;
        public string employeeName;

        private decimal originalTotalDue;
        private decimal totalDue;

        private string paymentMethod = string.Empty;

        private string lastCardBrand;
        private string lastCardLastFour;
        private int lastExpMonth;
        private int lastExpYear;
        private string? lastChargeId;
        private string lastTransactionStatus;
        private string transactionID;
        private int transactionNumber;
        private decimal discounts = 0;



        public PointOfSaleWindow(string employeeID, string employeeName)
        {
            InitializeComponent();

            transactionSummary = new TransactionSummary();
            this.DataContext = this;

            productRepository = new ProductRepository(databaseHelper.GetConnectionString());
            SaleItems = new ObservableCollection<InventoryItem>();
            ReturnItems = new ObservableCollection<InventoryItem>();
            TradeItems = new ObservableCollection<TradeItem>();
            SummaryItems = new ObservableCollection<SummaryItem>();
            Customer = new ObservableCollection<Customer> ();

            this.employeeID = employeeID;
            this.employeeName = employeeName;

            txtEmployeeName.Text = employeeName;
            txtEmployeeID.Text = employeeID;

            // Initialize Quick Select
            InitializeQuickSelect();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(SummaryItems);
            view.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));

            LoadLocationAndRegisterSettings();
            int transactionNumber = GetNextTransactionNumber();
            txtTransactionNumber.Text = transactionNumber.ToString();

            LoadTaxRate();
            UpdateTotals();

            transactionID = GenerateTransactionID();

            txtTotal.DataContext = transactionSummary;
        }

        private void InitializeQuickSelect()
        {
            QuickSelectProductGrid.Children.Clear();
            QuickSelectComboGrid.Children.Clear();

            var productRepository = new ProductRepository(databaseHelper.GetConnectionString());

            // Load individual items (Products) from settings
            if (Properties.Settings.Default.QuickSelectItems != null)
            {
                foreach (string sku in Properties.Settings.Default.QuickSelectItems)
                {
                    var product = productRepository.GetProductBySKU(sku);
                    if (product != null)
                    {
                        Button itemButton = new Button
                        {
                            Content = product.ProductName, // Display product name
                            Margin = new Thickness(5),
                            Tag = product.SKU,
                            ToolTip = $"Quick select product: {product.ProductName}" // Tooltip for clarity
                        };
                        itemButton.Click += OnQuickSelectItem_Click;
                        QuickSelectProductGrid.Children.Add(itemButton);
                    }
                }
            }

            // Load combo items from settings
            if (Properties.Settings.Default.QuickSelectCombos != null)
            {
                foreach (string comboSKU in Properties.Settings.Default.QuickSelectCombos)
                {
                    var combo = productRepository.GetComboBySKU(comboSKU);
                    if (combo != null)
                    {
                        Button comboButton = new Button
                        {
                            Content = combo.ComboName, // Display combo name
                            Margin = new Thickness(5),
                            Tag = combo.ComboSKU,
                            ToolTip = $"Quick select combo: {combo.ComboName}" // Tooltip for clarity
                        };
                        comboButton.Click += OnQuickSelectCombo_Click;
                        QuickSelectComboGrid.Children.Add(comboButton);
                    }
                }
            }
        }





        public void RefreshQuickSelect()
        {
            InitializeQuickSelect();
        }




        private void LoadTaxRate()
        {
            // Load tax rate from settings (assuming it's stored as a decimal)
            taxRate = Properties.Settings.Default.TaxRate / 100;
        }

        private void OnBtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string query = txtSearchSKU.Text.Trim();
            if (!string.IsNullOrEmpty(query))
            {
                var products = productRepository.SearchProducts(query);
                var combos = productRepository.SearchCombos(query); // New method to search combos

                // Combine products and combos into a single collection for display
                var combinedResults = products.Cast<object>().Concat(combos.Cast<object>()).ToList();
                cbSearchResults.ItemsSource = combinedResults;
                cbSearchResults.Items.Refresh();
            }
        }

        public List<Product> SearchProducts(string query)
        {
            string sql = @"
        SELECT c.SKU, c.ProductName, c.Price, c.CategoryID
        FROM Catalog c
        WHERE c.SKU LIKE @query OR c.ProductName LIKE @query";

            List<Product> products = new List<Product>();

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@query", $"%{query}%");
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                CategoryID = reader["CategoryID"].ToString()?.Trim() // Trim CategoryID
                            });
                        }
                    }
                }
            }

            return products;
        }


        public List<Combo> SearchCombos(string query)
        {
            string connectionString = databaseHelper.GetConnectionString();
            List<Combo> combos = new List<Combo>();

            string sql = @"SELECT ComboSKU, ComboName, ComboPrice 
                   FROM Combos 
                   WHERE ComboSKU LIKE @query OR ComboName LIKE @query";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@query", $"%{query}%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            combos.Add(new Combo
                            {
                                ComboSKU = reader["ComboSKU"].ToString(),
                                ComboName = reader["ComboName"].ToString(),
                                ComboPrice = Convert.ToDecimal(reader["ComboPrice"])
                            });
                        }
                    }
                }
            }

            return combos;
        }


        private void OnAddToSale_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var context = button?.DataContext;

            if (context is Product product)
            {
                AddProductToSale(product);
            }
            else if (context is Combo combo)
            {
                AddComboToSale(combo);
            }
        }

        public void AddProductToSale(Product product)
        {
            // Check if the product is serialized
            bool isSerialized = IsSerializedProduct(product.SKU);
            string serialNumber = null;

            if (isSerialized)
            {
                // Prompt for serial number
                serialNumber = PromptForSerialNumber(product.SKU);
                if (string.IsNullOrEmpty(serialNumber))
                {
                    MessageBox.Show("Please enter a valid serial number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var saleItem = new InventoryItem
            {
                SKU = product.SKU,
                ProductName = product.ProductName,
                Price = product.Price,
                ProductSerialNumber = serialNumber,
                CategoryID = product.CategoryID?.Trim()
            };

            SaleItems.Add(saleItem);
            lvSaleItems.ItemsSource = SaleItems;
            lvSaleItems.Items.Refresh();

            AddToSummary("Sale", saleItem, saleItem.Price, 1, false);
            UpdateTotals();
        }

        private void AddComboToSale(Combo combo)
        {
            var comboItems = productRepository.GetComboItems(combo.ComboSKU);
            bool isComboValid = true;

            foreach (var comboItem in comboItems)
            {
                if (string.IsNullOrEmpty(comboItem.SKU))
                {
                    // Handle placeholder items
                    string categoryID = comboItem.CategoryID;
                    string categoryName = productRepository.GetCategoryNameByID(categoryID);
                    string inputSKU = Microsoft.VisualBasic.Interaction.InputBox(
                        $"Enter a SKU for category '{categoryName}' (Category ID: {categoryID}).",
                        "Enter SKU");

                    if (string.IsNullOrEmpty(inputSKU) || !inputSKU.StartsWith(categoryID))
                    {
                        MessageBox.Show("Invalid SKU. Combo not added.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        isComboValid = false;
                        break;
                    }

                    var selectedProduct = productRepository.GetProductBySKU(inputSKU);
                    if (selectedProduct == null)
                    {
                        MessageBox.Show("SKU not found in catalog.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        isComboValid = false;
                        break;
                    }

                    comboItem.SKU = selectedProduct.SKU; // Assign the selected product's SKU to the combo item
                    comboItem.Price = selectedProduct.Price; // Assign correct price
                    comboItem.ProductName = selectedProduct.ProductName;
                }

                var saleItem = CreateInventoryItemFromProduct(comboItem);
                saleItem.IsPartOfCombo = true;
                SaleItems.Add(saleItem);
            }

            if (!isComboValid)
            {
                SaleItems.Clear(); // Clear items if combo validation failed
                MessageBox.Show("Combo not added due to errors.", "Combo Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add all combo items to the summary
            foreach (var item in comboItems)
            {
                AddToSummary("Sale", item, item.Price, item.Quantity, false);
            }

            lvSaleItems.ItemsSource = SaleItems;
            lvSaleItems.Items.Refresh();
            lvSummary.Items.Refresh();
            UpdateTotals();
        }

        public void AddServicesToTransaction(Appointment appointment)
        {
            bool hasInvalidItems = false;

            // Add services
            foreach (var service in appointment.Services)
            {
                if (!string.IsNullOrEmpty(service.ServiceID) && service.ServicePrice > 0)
                {
                    var saleItem = new InventoryItem
                    {
                        SKU = service.ServiceID,
                        ProductName = service.ServiceName,
                        Price = service.ServicePrice
                    };

                    SaleItems.Add(saleItem);
                    AddToSummary("Sale", saleItem, saleItem.Price, 1, false);
                }
                else
                {
                    hasInvalidItems = true;

                    // Debug information to identify the invalid service
                    string debugInfo = $"ServiceID: {service.ServiceID}, ServiceName: {service.ServiceName}, ServicePrice: {service.ServicePrice}";
                    MessageBox.Show($"Invalid service item: {service.ServiceName}. Ensure SKU and price are valid.\n\nDetails: {debugInfo}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Add service add-ons
            foreach (var addOn in appointment.ServiceAddOns)
            {
                if (!string.IsNullOrEmpty(addOn.ServicePlusID) && addOn.ServicePlusPrice > 0)
                {
                    var saleItem = new InventoryItem
                    {
                        SKU = addOn.ServicePlusID,
                        ProductName = addOn.ServicePlusName,
                        Price = addOn.ServicePlusPrice
                    };

                    SaleItems.Add(saleItem);
                    AddToSummary("Sale", saleItem, saleItem.Price, 1, false);
                }
                else
                {
                    hasInvalidItems = true;

                    // Debug information to identify the invalid add-on
                    string debugInfo = $"ServicePlusID: {addOn.ServicePlusID}, ServicePlusName: {addOn.ServicePlusName}, ServicePlusPrice: {addOn.ServicePlusPrice}";
                    MessageBox.Show($"Invalid add-on item: {addOn.ServicePlusName}. Ensure SKU and price are valid.\n\nDetails: {debugInfo}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Add service fees
            foreach (var fee in appointment.ServiceFees)
            {
                if (!string.IsNullOrEmpty(fee.ServiceFeeID) && fee.ServiceFeePrice > 0)
                {
                    var saleItem = new InventoryItem
                    {
                        SKU = fee.ServiceFeeID,
                        ProductName = fee.ServiceFeeName,
                        Price = fee.ServiceFeePrice
                    };

                    SaleItems.Add(saleItem);
                    AddToSummary("Sale", saleItem, saleItem.Price, 1, false);
                }
                else
                {
                    hasInvalidItems = true;

                    // Debug information to identify the invalid fee
                    string debugInfo = $"ServiceFeeID: {fee.ServiceFeeID}, ServiceFeeName: {fee.ServiceFeeName}, ServiceFeePrice: {fee.ServiceFeePrice}";
                    MessageBox.Show($"Invalid fee item: {fee.ServiceFeeName}. Ensure SKU and price are valid.\n\nDetails: {debugInfo}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (!hasInvalidItems)
            {
                MessageBox.Show("All services, add-ons, and fees have been added to the transaction.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Refresh UI
            lvSaleItems.ItemsSource = SaleItems;
            lvSaleItems.Items.Refresh();
            lvSummary.Items.Refresh();
            UpdateTotals();
        }




        private InventoryItem CreateInventoryItemFromProduct(Product product)
        {
            return new InventoryItem
            {
                SKU = product.SKU,
                ProductName = product.ProductName,
                Price = product.Price,
                CategoryID = product.CategoryID,
                IsPartOfCombo = false // Default, unless set otherwise
            };
        }




        // Add product to TradeItems
        public void OnAddToTrade_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as Product;

            if (product != null)
            {
                // Check if the product is serialized
                bool isSerialized = IsSerializedProduct(product.SKU);
                string serialNumber = null;

                if (isSerialized)
                {
                    // Prompt for serial number
                    serialNumber = PromptForSerialNumber(product.SKU);
                    if (string.IsNullOrEmpty(serialNumber))
                    {
                        MessageBox.Show("Please enter a valid serial number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var tradeItem = new TradeItem
                {
                    SKU = product.SKU,
                    ProductName = product.ProductName,
                    TradeValue = product.Price,
                    ProductSerialNumber = serialNumber,
                    CategoryID = product.CategoryID
                };

                TradeItems.Add(tradeItem);
                lvTradeItems.ItemsSource = TradeItems;
                lvTradeItems.Items.Refresh();

                AddToSummary("Trade", tradeItem, tradeItem.TradeValue, 1, false);
                UpdateTotals();
            }
        }


        // Add product to ReturnItems
        private void OnAddToReturn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as Product;

            if (product != null)
            {
                var returnItem = new InventoryItem
                {
                    SKU = product.SKU,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    CategoryID = product.CategoryID
                };

                ReturnItems.Add(returnItem);
                lvReturnItems.ItemsSource = ReturnItems;
                lvReturnItems.Items.Refresh();

                AddToSummary("Return", returnItem, returnItem.Price, 1, false);
                UpdateTotals();
            }
        }


        // Remove from SaleItems
        private void OnRemoveFromSale_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var inventoryItem = button?.DataContext as InventoryItem;

            if (inventoryItem != null)
            {
                SaleItems.Remove(inventoryItem);
                lvSaleItems.Items.Refresh();
                RemoveFromSummary(inventoryItem, "Sale");
            }
            UpdateTotals();
        }

        // Remove from TradeItems
        private void OnRemoveFromTrade_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tradeItem = button?.DataContext as TradeItem;

            if (tradeItem != null)
            {
                TradeItems.Remove(tradeItem);
                lvTradeItems.Items.Refresh();
                RemoveFromSummary(tradeItem, "Trade");
            }
            UpdateTotals();
        }

        // Remove from ReturnItems
        private void OnRemoveFromReturn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var returnItem = button?.DataContext as InventoryItem;

            if (returnItem != null)
            {
                ReturnItems.Remove(returnItem);
                lvReturnItems.Items.Refresh();
                RemoveFromSummary(returnItem, "Return");
            }
            UpdateTotals();
        }

        private void OnRemoveFromSummary_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var summaryItem = button?.DataContext as SummaryItem;

            if (summaryItem != null)
            {
                if (summaryItem.Type == "Sale")
                {
                    // Check if this summary item is a combo or part of a combo
                    bool isComboItem = SummaryItems.Any(s => s.SKU == summaryItem.SKU && s.Type == "Sale");

                    if (isComboItem)
                    {
                        // Remove combo items from SaleItems
                        var comboItemsToRemove = SaleItems.Where(item => item.SKU == summaryItem.SKU || item.IsPartOfCombo).ToList();

                        foreach (var comboItem in comboItemsToRemove)
                        {
                            SaleItems.Remove(comboItem);
                        }

                        // Remove combo itself from SummaryItems
                        var comboSummaryItem = SummaryItems.FirstOrDefault(s => s.SKU == summaryItem.SKU && s.Type == "Sale");
                        if (comboSummaryItem != null)
                        {
                            SummaryItems.Remove(comboSummaryItem);
                        }
                    }
                    else
                    {
                        // Handle normal sale item removal
                        var saleItem = SaleItems.FirstOrDefault(item => item.SKU == summaryItem.SKU && !item.IsPartOfCombo);
                        if (saleItem != null)
                        {
                            SaleItems.Remove(saleItem);

                            // Also remove the summary entry
                            var summarySingleItem = SummaryItems.FirstOrDefault(s => s.SKU == saleItem.SKU && s.Type == "Sale");
                            if (summarySingleItem != null)
                            {
                                SummaryItems.Remove(summarySingleItem);
                            }
                        }
                    }
                }
                else
                {
                    // Handle Trade and Return items based on their type
                    switch (summaryItem.Type)
                    {
                        case "Trade":
                            var tradeItem = TradeItems.FirstOrDefault(item => item.SKU == summaryItem.SKU);
                            if (tradeItem != null)
                            {
                                TradeItems.Remove(tradeItem);
                                SummaryItems.Remove(summaryItem);
                            }
                            break;

                        case "Return":
                            var returnItem = ReturnItems.FirstOrDefault(item => item.SKU == summaryItem.SKU);
                            if (returnItem != null)
                            {
                                ReturnItems.Remove(returnItem);
                                SummaryItems.Remove(summaryItem);
                            }
                            break;
                    }
                }

                // Refresh the ListViews
                lvSaleItems.Items.Refresh();
                lvTradeItems.Items.Refresh();
                lvReturnItems.Items.Refresh();
                lvSummary.Items.Refresh();

                // Update totals after removal
                UpdateTotals();
            }
        }


        public void AddToSummary(string type, Product item, decimal value, int quantity, bool isDefective)
        {
            if (item == null || value <= 0)
            {
                MessageBox.Show($"Invalid item or price for SKU: {item?.SKU}. Ensure all products have correct pricing.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var summaryItem = new SummaryItem
            {
                SKU = item.SKU,
                ProductName = item.ProductName,
                Value = value,
                AdjustedValue = value,
                Quantity = quantity,
                Type = type,
                IsDefective = isDefective,
                CategoryID = item.CategoryID // Ensure CategoryID is passed here
            };

            SummaryItems.Add(summaryItem);
            lvSummary.Items.Refresh(); // Refresh UI
        }




        // Removes an item from the SummaryItems collection
        private void RemoveFromSummary(InventoryItem item, string type)
        {
            var summaryItem = SummaryItems.FirstOrDefault(s => s.SKU == item.SKU && s.Type == type);
            if (summaryItem != null)
            {
                SummaryItems.Remove(summaryItem);
            }
        }

        private void LoadLocationAndRegisterSettings()
        {
            // Get LocationID and RegisterNumber from application settings
            string locationID = Properties.Settings.Default.LocationID;
            string registerNumber = Properties.Settings.Default.RegisterNumber;

            // Set the TextBlocks to display these values
            txtLocationID.Text = !string.IsNullOrEmpty(locationID) ? locationID : "Not Set";
            txtRegisterNumber.Text = !string.IsNullOrEmpty(registerNumber) ? registerNumber : "Not Set";
        }

        private void UpdateTotals()
        {
            // Recalculate the subtotal based on the loaded SummaryItems
            subtotal = SummaryItems
                .Where(item => item.Type == "Sale" && item.AdjustedValue > 0)
                .Sum(item => item.AdjustedValue * item.Quantity);

            returnsSubtotal = SummaryItems
                .Where(item => item.Type == "Return")
                .Sum(item => item.AdjustedValue * item.Quantity);

            decimal salesTax = subtotal * taxRate;
            decimal returnsTax = returnsSubtotal * taxRate;
            taxes = salesTax - returnsTax;

            total = subtotal + taxes - returnsSubtotal;

            // Calculate total discounts
            decimal totalDiscounts = SummaryItems.Sum(item => item.DiscountAmount);

            transactionSummary.Total = total; // Update transaction summary binding

            // Update UI fields
            txtSubtotal.Text = subtotal.ToString("C2");
            txtReturnsSubtotal.Text = returnsSubtotal.ToString("C2");
            txtTaxes.Text = taxes.ToString("C2");
            txtDiscounts.Text = totalDiscounts.ToString("C2");
            txtTotal.Text = total.ToString("C2");
        }


        public void AddToTrade(InventoryItem item)
        {
            // Existing logic from OnAddToTrade_Click but without event arguments
            bool isSerialized = IsSerializedProduct(item.SKU);
            string serialNumber = null;

            if (isSerialized)
            {
                serialNumber = PromptForSerialNumber(item.SKU);
                if (string.IsNullOrEmpty(serialNumber))
                {
                    MessageBox.Show("Please enter a valid serial number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var tradeItem = new TradeItem
            {
                SKU = item.SKU,
                ProductName = item.ProductName,
                TradeValue = item.Price,
                ProductSerialNumber = serialNumber,
                CategoryID = item.CategoryID
            };

            TradeItems.Add(tradeItem);
            lvTradeItems.ItemsSource = TradeItems;
            lvTradeItems.Items.Refresh();

            AddToSummary("Trade", tradeItem, tradeItem.TradeValue, 1, false);
            UpdateTotals();
        }





        private void OnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerWindow addCustomerWindow = new AddCustomerWindow();
            bool? result = addCustomerWindow.ShowDialog();

            // If a customer was successfully added, update the customer display
            if (result == true)
            {
                Customer newlyAddedCustomer = GetLastAddedCustomer();
                DisplayCustomerInfo(newlyAddedCustomer);
            }
        }

        // Event handler for searching customers
        private void OnSearchCustomer_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = txtCustomerSearch.Text.Trim(); // Get the search term from the input field

            // Open the CustomerSearchWindow with the search term pre-populated
            CustomerSearchWindow customerSearchWindow = new CustomerSearchWindow(searchTerm);
            bool? result = customerSearchWindow.ShowDialog();

            // If a customer is selected, populate the customer info
            if (result == true)
            {
                Customer selectedCustomer = customerSearchWindow.SelectedCustomer;
                DisplayCustomerInfo(selectedCustomer);
            }
        }



        // Method to get the last added customer (use appropriate query for your DB)
        private Customer GetLastAddedCustomer()
        {
            string query = "SELECT TOP 1 * FROM Customers ORDER BY CustomerID DESC";
            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                CustomerID = reader["CustomerID"].ToString(),
                                CustomerFirstName = reader["CustomerFirstName"].ToString(),
                                CustomerLastName = reader["CustomerLastName"].ToString(),
                                CustomerPhoneNumber = reader["CustomerPhoneNumber"].ToString(),
                                CustomerEmail = reader["CustomerEmail"].ToString(),
                                CustomerLoyalty = Convert.ToBoolean(reader["CustomerLoyalty"]),
                                CustomerPoints = Convert.ToInt32(reader["CustomerPoints"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Method to search customer by a search term (e.g., name, phone number, etc.)
        private Customer SearchCustomer(string searchTerm)
        {
            string query = @"SELECT * FROM Customers WHERE 
                            CustomerFirstName LIKE @searchTerm OR 
                            CustomerLastName LIKE @searchTerm OR 
                            CustomerPhoneNumber LIKE @searchTerm";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                CustomerID = reader["CustomerID"].ToString(),
                                CustomerFirstName = reader["CustomerFirstName"].ToString(),
                                CustomerLastName = reader["CustomerLastName"].ToString(),
                                CustomerPhoneNumber = reader["CustomerPhoneNumber"].ToString(),
                                CustomerEmail = reader["CustomerEmail"].ToString(),
                                CustomerLoyalty = Convert.ToBoolean(reader["CustomerLoyalty"]),
                                CustomerPoints = Convert.ToInt32(reader["CustomerPoints"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Method to display customer info on the PointOfSaleWindow
        private void DisplayCustomerInfo(Customer customer)
        {
            txtFirstName.Text = customer.CustomerFirstName;
            txtLastName.Text = customer.CustomerLastName;
            txtCustomerID.Text = customer.CustomerID;
            txtPhoneNumber.Text = customer.CustomerPhoneNumber;
            txtEmail.Text = customer.CustomerEmail;
            txtLoyalty.Text = customer.CustomerPoints.ToString();
        }

        private void OnBtnViewEditTransaction_Click(object sender, RoutedEventArgs e)
        {
            // Pass the current list of SummaryItems to the ViewEditTransactionDialogWindow
            ViewEditTransactionDialogWindow viewEditTransactionDialogWindow = new ViewEditTransactionDialogWindow(SummaryItems.ToList());
            viewEditTransactionDialogWindow.Owner = this; // Set the owner to this window
            viewEditTransactionDialogWindow.ShowDialog();
        }
        public void RefreshSummaryItems(List<SummaryItem> updatedItems)
        {
            SummaryItems.Clear();
            foreach (var item in updatedItems)
            {
                SummaryItems.Add(item);
            }

            lvSummary.Items.Refresh(); // Refresh the UI after updating

            UpdateTotals(); // Recalculate totals after discount is applied
        }

        private int GetNextTransactionNumber()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd"); // Get today's date in the format YYYY-MM-DD
            int nextTransactionNumber = 1; // Default to 1 if no transactions for today

            string query = @"SELECT ISNULL(MAX(TransactionNumber), 0) + 1
                     FROM Transactions
                     WHERE CAST(TransactionDate AS DATE) = @Today";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Today", today);

                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        nextTransactionNumber = Convert.ToInt32(result);
                    }
                }
            }

            return nextTransactionNumber;
        }

        // Method to check if the product is serialized
        private bool IsSerializedProduct(string sku)
        {
            string query = @"SELECT COUNT(*) 
                     FROM CategoryTraits CT 
                     INNER JOIN Catalog C ON C.CategoryID = CT.CategoryID
                     WHERE C.SKU = @SKU AND CT.TraitName = 'Serialized'";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SKU", sku);
                    int result = (int)cmd.ExecuteScalar();
                    return result > 0;
                }
            }
        }

        // Method to prompt the user for a serial number
        private string PromptForSerialNumber(string sku)
        {
            return Microsoft.VisualBasic.Interaction.InputBox($"Enter the serial number for SKU: {sku}", "Enter Serial Number", "");
        }


        private string GenerateTransactionID()
        {
            return Guid.NewGuid().ToString();
        }

        private void SetMerlinId()
        {
            transactionID = Guid.NewGuid().ToString();

        }

        private void OnApplyPromotions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadActivePromotions(); // Fetch active promotions
                ApplyPromotionsToSummary();
                UpdateTotals();

                // Get a list of applied promotions for display
                var appliedPromotions = SummaryItems
                    .Where(item => !string.IsNullOrEmpty(item.PromotionsApplied))
                    .Select(item => $"{item.ProductName}: {item.PromotionsApplied}")
                    .ToList();

                string message = appliedPromotions.Any()
                    ? string.Join("\n", appliedPromotions)
                    : "No promotions applied.";

                MessageBox.Show($"Promotions applied successfully!\n\n{message}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying promotions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadActivePromotions()
        {
            string query = @"
        SELECT PromotionID, PromotionName, PromotionStartDate, PromotionEndDate,
               PromotionDiscountValue, IsPromotionPercentageValue, IsPromotionDollarValue,
               PromotionTargetCategory, PromotionTargetSKU, IsPromotionCodeActivated, PromotionActivationCode
        FROM Promotions
        WHERE GETDATE() BETWEEN PromotionStartDate AND PromotionEndDate";

            activePromotions = new List<Promotion>();

            using (var conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var promotion = new Promotion
                            {
                                PromotionID = reader["PromotionID"].ToString(),
                                PromotionName = reader["PromotionName"].ToString(),
                                PromotionStartDate = Convert.ToDateTime(reader["PromotionStartDate"]),
                                PromotionEndDate = Convert.ToDateTime(reader["PromotionEndDate"]),
                                PromotionDiscountValue = Convert.ToDecimal(reader["PromotionDiscountValue"]),
                                IsPercentage = Convert.ToBoolean(reader["IsPromotionPercentageValue"]),
                                IsDollarValue = Convert.ToBoolean(reader["IsPromotionDollarValue"]),
                                IsCodeActivated = Convert.ToBoolean(reader["IsPromotionCodeActivated"]),
                                ActivationCode = reader["PromotionActivationCode"].ToString()
                            };

                            string targetCategory = reader["PromotionTargetCategory"]?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(targetCategory))
                            {
                                promotion.ApplicableCategories.Add(targetCategory);
                            }

                            string targetSKU = reader["PromotionTargetSKU"]?.ToString()?.Trim();
                            if (!string.IsNullOrEmpty(targetSKU))
                            {
                                promotion.ApplicableSKUs.Add(targetSKU);
                            }

                            activePromotions.Add(promotion);
                        }
                    }
                }
            }
        }
        private void ApplyPromotionsToSummary()
        {
            foreach (var summaryItem in SummaryItems)
            {
                summaryItem.AdjustedValue = summaryItem.Value;
                summaryItem.DiscountAmount = 0;
                summaryItem.PromotionsApplied = string.Empty; // Clear existing promotions for re-evaluation

                var applicablePromotions = activePromotions.Where(p =>
                    p.IsApplicableToCategory(summaryItem.CategoryID) ||
                    p.IsApplicableToSKU(summaryItem.SKU)).ToList();

                foreach (var promotion in applicablePromotions)
                {
                    if (promotion.IsCodeActivated)
                    {
                        string userCode = Microsoft.VisualBasic.Interaction.InputBox(
                            $"Enter activation code for promotion: {promotion.PromotionName}",
                            "Promotion Activation");

                        // Trim and compare case-insensitively
                        if (!string.Equals(userCode.Trim(), promotion.ActivationCode.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show($"Invalid code for promotion: {promotion.PromotionName}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }
                    }

                    decimal discount = promotion.IsPercentage
                        ? summaryItem.AdjustedValue * (promotion.PromotionDiscountValue / 100)
                        : promotion.PromotionDiscountValue;

                    discount = Math.Min(discount, summaryItem.AdjustedValue);

                    summaryItem.AdjustedValue -= discount;
                    summaryItem.DiscountAmount += discount;

                    // Concatenate promotion names with commas
                    if (!string.IsNullOrEmpty(summaryItem.PromotionsApplied))
                    {
                        summaryItem.PromotionsApplied += ", ";
                    }
                    summaryItem.PromotionsApplied += promotion.PromotionName;
                }
            }

            lvSummary.Items.Refresh(); // Refresh the ListView
            UpdateTotals(); // Update totals to reflect changes
        }


        private void OnBtnCheckOut_Click(object sender, RoutedEventArgs e)
        {
            decimal totalDue = total;
           

        }

        private void OnQuickSelectItem_Click(object sender, RoutedEventArgs e)
        {
            string sku = (string)(sender as Button).Tag;

            // Fetch the product by SKU
            var product = productRepository.GetProductBySKU(sku);
            if (product != null)
            {
                AddProductToSale(product);
            }
            else
            {
                MessageBox.Show($"Product with SKU {sku} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnQuickSelectCombo_Click(object sender, RoutedEventArgs e)
        {
            string comboSKU = (string)(sender as Button).Tag;

            // Fetch the combo by SKU
            var combo = productRepository.GetComboBySKU(comboSKU);
            if (combo != null)
            {
                AddComboToSale(combo);
            }
            else
            {
                MessageBox.Show($"Combo with SKU {comboSKU} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnToggleQuickSelect_Click(object sender, RoutedEventArgs e)
        {
            if (QuickSelectPanel.Visibility == Visibility.Visible)
            {
                QuickSelectPanel.Visibility = Visibility.Collapsed;
                btnToggleQuickSelect.Content = "Show Quick Select";
            }
            else
            {
                QuickSelectPanel.Visibility = Visibility.Visible;
                btnToggleQuickSelect.Content = "Hide Quick Select";
            }
        }

        private void pointOfSaleFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }







        public void StorePaymentDetails (string cardBrand, string cardLastFour, int expMonth, int expYear, string chargeId)
        {
            this.lastCardBrand = cardBrand;
            this.lastCardLastFour = cardLastFour;
            this.lastExpMonth = expMonth;
            this.lastExpYear = expYear;
            this.paymentMethod = "Card";
            this.lastChargeId = chargeId;
        }

        private void OnSuspendTransaction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Generate a unique TransactionID and fetch the next TransactionNumber
                string transactionID = Guid.NewGuid().ToString();
                int transactionNumber = GetNextTransactionNumber();

                // Insert the transaction into the Transactions table
                string query = @"
        INSERT INTO Transactions (
            TransactionID, TransactionNumber, RegisterNumber, LocationID, TransactionDate, 
            TransactionTime, EmployeeID, CustomerID, Subtotal, Taxes, TotalAmount, 
            NetCash, IsSuspended
        )
        VALUES (
            @TransactionID, @TransactionNumber, @RegisterNumber, @LocationID, @TransactionDate, 
            @TransactionTime, @EmployeeID, @CustomerID, @Subtotal, @Taxes, @TotalAmount, 
            @NetCash, @IsSuspended
        )";

                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransactionID", transactionID);
                        cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                        cmd.Parameters.AddWithValue("@RegisterNumber", Properties.Settings.Default.RegisterNumber);
                        cmd.Parameters.AddWithValue("@LocationID", Properties.Settings.Default.LocationID);
                        cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@TransactionTime", DateTime.Now.TimeOfDay);
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        cmd.Parameters.AddWithValue("@CustomerID", string.IsNullOrWhiteSpace(txtCustomerID.Text) ? DBNull.Value : txtCustomerID.Text.Trim());
                        cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                        cmd.Parameters.AddWithValue("@Taxes", taxes);
                        cmd.Parameters.AddWithValue("@TotalAmount", total);
                        cmd.Parameters.AddWithValue("@NetCash", 0); // No payment yet
                        cmd.Parameters.AddWithValue("@IsSuspended", 1); // Mark as suspended

                        cmd.ExecuteNonQuery();
                    }

                    // Save associated transaction details
                    SaveTransactionDetails(transactionID, transactionNumber);
                }

                MessageBox.Show("Transaction suspended successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear the current transaction UI
                ClearTransaction();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error suspending transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void SaveTransactionDetails(string transactionID, int transactionNumber)
        {
            string query = @"
        INSERT INTO TransactionDetails (
            TransactionID, TransactionNumber, RegisterNumber, LocationID, TransactionDate, 
            SKU, CategoryID, Quantity, Price, DiscountLoyalty, DiscountManual, Description
        )
        VALUES (
            @TransactionID, @TransactionNumber, @RegisterNumber, @LocationID, @TransactionDate, 
            @SKU, @CategoryID, @Quantity, @Price, @DiscountLoyalty, @DiscountManual, @Description
        )";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    foreach (var item in SummaryItems)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@TransactionID", transactionID);
                        cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                        cmd.Parameters.AddWithValue("@RegisterNumber", Properties.Settings.Default.RegisterNumber);
                        cmd.Parameters.AddWithValue("@LocationID", Properties.Settings.Default.LocationID);
                        cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now.Date);
                        cmd.Parameters.AddWithValue("@SKU", item.SKU);
                        cmd.Parameters.AddWithValue("@CategoryID", item.CategoryID);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@Price", item.Price);
                        cmd.Parameters.AddWithValue("@DiscountLoyalty", item.LoyaltyDiscountedValue);
                        cmd.Parameters.AddWithValue("@DiscountManual", item.ManualDiscountedValue);
                        cmd.Parameters.AddWithValue("@Description", item.ProductName);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void LoadSuspendedTransaction(string transactionID)
        {
            try
            {


                // Load transaction details into SummaryItems
                PopulateSummaryItemsFromTransaction(transactionID);

                // Fetch transaction metadata (e.g., Subtotal, Taxes)
                string query = @"
        SELECT TransactionNumber, Subtotal, Taxes, TotalAmount
        FROM Transactions
        WHERE TransactionID = @TransactionID";

                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TransactionID", transactionID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtTransactionNumber.Text = reader["TransactionNumber"].ToString();
                                subtotal = Convert.ToDecimal(reader["Subtotal"]);
                                taxes = Convert.ToDecimal(reader["Taxes"]);
                                total = Convert.ToDecimal(reader["TotalAmount"]);

                                txtSubtotal.Text = subtotal.ToString("C2");
                                txtTaxes.Text = taxes.ToString("C2");
                                txtTotal.Text = total.ToString("C2");
                            }
                        }
                    }
                }

                MessageBox.Show("Transaction resumed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resuming transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Helper method to load summary items from database
        private void PopulateSummaryItemsFromTransaction(string transactionID)
        {
            string query = @"
        SELECT SKU, CategoryID, Quantity, Price, Description 
        FROM TransactionDetails
        WHERE TransactionID = @TransactionID";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TransactionID", transactionID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SummaryItems.Add(new SummaryItem
                            {
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["Description"].ToString(),
                                Value = Convert.ToDecimal(reader["Price"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Type = "Sale" // Adjust as necessary
                            });
                        }
                    }
                }
            }
        }

        private void FinalizeTransactionPage_TotalDueUpdated(decimal newTotalDue)
        {
            txtTotal.Text = $"${newTotalDue:F2}"; // Update the text block with new total due
        }


        private void ClearTransaction()
        {

            this.Close();
        }




    }


}