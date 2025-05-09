using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Repositories;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Windows;
using MerlinPointOfSale.Windows.DialogWindows;
using MerlinPointOfSale.Pages.ReleasePointOfSalePages;
using System.Windows.Navigation;
using System.Data.SqlClient;
using MerlinPointOfSale.Properties;
using MerlinPointOfSale.Helpers;

namespace MerlinPointOfSale.Controls
{
    public partial class MainSearchBar : UserControl, INotifyPropertyChanged
    {
        private readonly ProductRepository productRepository;
        private EventHelper eventHelper;
        private DatabaseHelper databaseHelper;
        public string locationID;

        public ObservableCollection<InventoryItem> Suggestions { get; private set; }
        public string SearchText { get; set; }

        private bool isPopupOpen;
        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set
            {
                if (isPopupOpen != value)
                {
                    isPopupOpen = value;
                    OnPropertyChanged();
                    if (popup != null)
                    {
                        popup.IsOpen = isPopupOpen;
                    }
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

        public MainSearchBar()
        {
            InitializeComponent();
            DataContext = this;
            locationID = Settings.Default.LocationID;

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
                double popupWidth = 800; // Updated width for the wider popup

                if (popup.Child is FrameworkElement popupChild)
                {
                    popupWidth = popupChild.ActualWidth > 0 ? popupChild.ActualWidth : 800;
                }

                PopupHorizontalOffset = (controlWidth - popupWidth) / 2.925;
            }
        }


        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string input = (sender as TextBox)?.Text.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                FilterSuggestions(input);
            }
            else
            {
                Suggestions.Clear();
            }

            IsPopupOpen = Suggestions.Count > 0;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            IsPopupOpen = Suggestions.Count > 0;
        }

        private void FilterSuggestions(string query)
        {
            Suggestions.Clear();

            try
            {
                var inventoryItems = productRepository.SearchInventory(query);

                foreach (var item in inventoryItems)
                {
                    Suggestions.Add(item);
                }

                IsPopupOpen = Suggestions.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching search results: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SuggestionClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.DataContext is InventoryItem item)
            {
                SearchText = $"{item.SKU} - {item.ProductName} (Qty: {item.QuantityOnHandSellable})";
                IsPopupOpen = false;

                // Optional: Add additional logic for when an item is selected
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnAddToSale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is InventoryItem item)
            {
                var loginWindow = new TransactionLoginWindow();
                if (loginWindow.ShowDialog() == true)
                {
                    // Proceed if the employee is verified
                    var saleWindow = new PointOfSaleWindow_Release(loginWindow.EmployeeID, loginWindow.EmployeeFirstName);
                    saleWindow.AddProductToSale(item); // Ensure this method is defined in PointOfSaleWindow
                    saleWindow.ShowDialog();
                }
                else
                {
                    //
                }
            }
        }

        private void OnAddToTrade_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is InventoryItem item)
            {
                var loginWindow = new TransactionLoginWindow();
                if (loginWindow.ShowDialog() == true)
                {
                    // Proceed if the employee is verified
                    var tradeWindow = new PointOfSaleWindow_Release(loginWindow.EmployeeID, loginWindow.EmployeeFirstName);
                    tradeWindow.AddToTrade(item); // Ensure this method is defined in PointOfSaleWindow
                    tradeWindow.ShowDialog();
                }
                else
                {
                    //
                }
            }
        }
        private void OnAddToReturn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is InventoryItem item)
            {
                var loginWindow = new TransactionLoginWindow();
                if (loginWindow.ShowDialog() == true)
                {
                    // Proceed if the employee is verified
                    var returnWindow = new PointOfSaleWindow_Release(loginWindow.EmployeeID, loginWindow.EmployeeFirstName);
                    returnWindow.AddToReturn(item); // Ensure this method exists in PointOfSaleWindow
                    returnWindow.ShowDialog();
                }
                else
                {
                    // Optionally handle cancel
                }
            }
        }



        private void OnPinToQuickSelect_Click(object sender, RoutedEventArgs e)
        {
            eventHelper = new EventHelper();
            if (sender is Button button && button.DataContext is InventoryItem item)
            {
                using (var connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();

                    var query = @"IF NOT EXISTS (SELECT 1 FROM LocationQuickSelect WHERE LocationID = @LocationID AND SKU = @SKU)
                          INSERT INTO LocationQuickSelect (LocationID, SKU) VALUES (@LocationID, @SKU)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", Settings.Default.LocationID);
                        command.Parameters.AddWithValue("@SKU", item.SKU);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"{item.ProductName.Trim()} has been added to Quick Select.", "Pinned", MessageBoxButton.OK, MessageBoxImage.Information);

                // Notify listeners
                EventHelper.Instance.RaiseQuickSelectUpdated();
            }
        }


    }


}
