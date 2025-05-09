using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Repositories;
using MerlinPointOfSale.Windows;
using MerlinPointOfSale.Windows.DialogWindows;

namespace MerlinPointOfSale.Controls
{
    public partial class PointOfSaleSearchBar : UserControl, INotifyPropertyChanged
    {
        private readonly ProductRepository productRepository;
        private readonly DatabaseHelper databaseHelper;
        private ObservableCollection<InventoryItem> suggestions;
        private bool isPopupOpen;

        public ObservableCollection<InventoryItem> Suggestions
        {
            get => suggestions;
            private set
            {
                suggestions = value;
                OnPropertyChanged();
            }
        }

        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set
            {
                if (isPopupOpen != value)
                {
                    isPopupOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        private double popupHorizontalOffset;
        public double PopupHorizontalOffset
        {
            get => popupHorizontalOffset;
            set
            {
                if (popupHorizontalOffset != value)
                {
                    popupHorizontalOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        public PointOfSaleSearchBar()
        {
            InitializeComponent();
            DataContext = this;

            Suggestions = new ObservableCollection<InventoryItem>();

            // Initialize database helper and repository
            databaseHelper = new DatabaseHelper();
            productRepository = new ProductRepository(databaseHelper.GetConnectionString());

            SearchBox.Loaded += (s, e) => AdjustPopupHorizontalOffset();
            SizeChanged += (s, e) => AdjustPopupHorizontalOffset();
        }
        private void AdjustPopupHorizontalOffset()
        {
            if (popup != null)
            {
                double controlWidth = this.ActualWidth;
                double popupWidth = 800; // Same width as MainSearchBar

                if (popup.Child is FrameworkElement popupChild)
                {
                    popupWidth = popupChild.ActualWidth > 0 ? popupChild.ActualWidth : 800;
                }

                PopupHorizontalOffset = (controlWidth - popupWidth) / 2.0;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            IsPopupOpen = Suggestions.Count > 0;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string input = (sender as TextBox)?.Text.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                PopulateSuggestions(input);
            }
            else
            {
                Suggestions.Clear();
                IsPopupOpen = false;
            }
        }

        private void PopulateSuggestions(string query)
        {
            Suggestions.Clear();
            try
            {
                // Fetch inventory items matching the query
                var inventoryItems = productRepository.SearchInventory(query);
                foreach (var item in inventoryItems)
                {
                    Suggestions.Add(item);
                }

                IsPopupOpen = Suggestions.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnAddToTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is InventoryItem item)
            {
                try
                {
                    // Check if the item is a BaseSKU
                    if (item.IsBaseSKU)
                    {
                        // Open the variant selection dialog
                        var variantDialog = new VariantSelectionDialog(item.SKU, databaseHelper.GetConnectionString());
                        if (variantDialog.ShowDialog() == true)
                        {
                            var selectedVariant = variantDialog.SelectedVariant;

                            if (selectedVariant != null)
                            {
                                // Add the selected variant to the transaction
                                var parentWindow = Window.GetWindow(this) as PointOfSaleWindow_Release;
                                parentWindow?.AddProductToSale(selectedVariant);
                            }
                            else
                            {
                                MessageBox.Show("No variant selected. Please try again.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }

                        return; // Prevent the BaseSKU from being added
                    }

                    // For non-BaseSKU items, add directly to the transaction
                    var parentWindowNormal = Window.GetWindow(this) as PointOfSaleWindow_Release;
                    parentWindowNormal?.AddProductToSale(item);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnAddToTrade_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is InventoryItem item)
            {

                var parentWindow = Window.GetWindow(this) as PointOfSaleWindow_Release;
                parentWindow.AddToTrade(item); // Ensure this method is defined in PointOfSaleWindow
            }
        }
        private void OnAddToReturn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is InventoryItem item)
            {

                var parentWindow = Window.GetWindow(this) as PointOfSaleWindow_Release;
                parentWindow.AddToReturn(item); // Ensure this method is defined in PointOfSaleWindow
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
