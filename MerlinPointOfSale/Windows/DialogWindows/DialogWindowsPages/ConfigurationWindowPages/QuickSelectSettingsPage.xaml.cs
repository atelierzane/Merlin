using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Repositories;

namespace MerlinPointOfSale.Windows.DialogWindows.DialogWindowsPages.ConfigurationWindowPages
{
    public partial class QuickSelectSettingsPage : Page
    {
        public ObservableCollection<(string SKU, string DisplayName)> IndividualItems { get; set; }
        public ObservableCollection<(string SKU, string DisplayName)> Combos { get; set; }

        private ProductRepository productRepository;

        // Event to notify when quick select settings are updated
        public event Action QuickSelectUpdated;

        public QuickSelectSettingsPage()
        {
            InitializeComponent();

            productRepository = new ProductRepository(new DatabaseHelper().GetConnectionString());

            // Load initial values from settings
            IndividualItems = LoadItemsFromSettings(Properties.Settings.Default.QuickSelectItems);
            Combos = LoadItemsFromSettings(Properties.Settings.Default.QuickSelectCombos);

            lstIndividualItems.ItemsSource = IndividualItems;
            lstCombos.ItemsSource = Combos;
        }

        private ObservableCollection<(string SKU, string DisplayName)> LoadItemsFromSettings(System.Collections.Specialized.StringCollection storedItems)
        {
            var collection = new ObservableCollection<(string SKU, string DisplayName)>();
            if (storedItems != null)
            {
                foreach (string sku in storedItems)
                {
                    var product = productRepository.GetProductBySKU(sku);
                    if (product != null)
                    {
                        collection.Add((sku, product.ProductName));
                    }
                    else
                    {
                        var combo = productRepository.GetComboBySKU(sku);
                        if (combo != null)
                        {
                            collection.Add((sku, combo.ComboName));
                        }
                    }
                }
            }
            return collection;
        }


        private void OnAddIndividualItem_Click(object sender, RoutedEventArgs e)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter SKU for the item:", "Add Product");
            if (!string.IsNullOrWhiteSpace(input) && IsProductValid(input, out string displayName))
            {
                IndividualItems.Add((input, displayName));
            }
            else
            {
                MessageBox.Show("Invalid or non-existent SKU.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnRemoveIndividualItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as (string SKU, string DisplayName)?;
            if (item.HasValue)
            {
                IndividualItems.Remove(item.Value);
            }
        }

        private void OnAddCombo_Click(object sender, RoutedEventArgs e)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Enter SKU for the combo:", "Add Combo");
            if (!string.IsNullOrWhiteSpace(input) && IsComboValid(input, out string displayName))
            {
                Combos.Add((input, displayName));
            }
            else
            {
                MessageBox.Show("Invalid or non-existent Combo SKU.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnRemoveCombo_Click(object sender, RoutedEventArgs e)
        {
            var combo = (sender as Button)?.DataContext as (string SKU, string DisplayName)?;
            if (combo.HasValue)
            {
                Combos.Remove(combo.Value);
            }
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.QuickSelectItems = new System.Collections.Specialized.StringCollection();
            foreach (var item in IndividualItems)
            {
                Properties.Settings.Default.QuickSelectItems.Add(item.SKU);
            }

            Properties.Settings.Default.QuickSelectCombos = new System.Collections.Specialized.StringCollection();
            foreach (var combo in Combos)
            {
                Properties.Settings.Default.QuickSelectCombos.Add(combo.SKU);
            }

            Properties.Settings.Default.Save();

            QuickSelectUpdated?.Invoke(); // Notify subscribers
            MessageBox.Show("Quick select settings saved successfully.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool IsProductValid(string sku, out string displayName)
        {
            var product = productRepository.GetProductBySKU(sku);
            if (product != null)
            {
                displayName = product.ProductName;
                return true;
            }
            displayName = string.Empty;
            return false;
        }

        private bool IsComboValid(string comboSKU, out string displayName)
        {
            var combo = productRepository.GetComboBySKU(comboSKU);
            if (combo != null)
            {
                displayName = combo.ComboName;
                return true;
            }
            displayName = string.Empty;
            return false;
        }
    }
}
