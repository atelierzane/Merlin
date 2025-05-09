using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Repositories;
using MerlinPointOfSale.Windows.DialogWindows;
using MerlinPointOfSale.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MerlinPointOfSale.Pages.ReleasePointOfSalePages
{
    public partial class QuickSelectPage : Page, INotifyPropertyChanged
    {
        private readonly ProductRepository productRepository;
        private readonly DatabaseHelper databaseHelper;
        private readonly Dictionary<string, (RowDefinition row, WrapPanel panel)> categorySections = new();

        private ObservableCollection<IGrouping<string, QuickSelectItem>> groupedQuickSelectItems;

        public ObservableCollection<IGrouping<string, QuickSelectItem>> GroupedQuickSelectItems
        {
            get => groupedQuickSelectItems;
            set
            {
                if (groupedQuickSelectItems != value)
                {
                    groupedQuickSelectItems = value;
                    OnPropertyChanged(nameof(GroupedQuickSelectItems));
                }
            }
        }

        public QuickSelectPage()
        {
            InitializeComponent();

            DataContext = this;

            databaseHelper = new DatabaseHelper();
            productRepository = new ProductRepository(databaseHelper.GetConnectionString());

            LoadQuickSelectItems();
            BuildCategorySections();
        }

        private void LoadQuickSelectItems()
        {
            try
            {
                string locationID = Properties.Settings.Default.LocationID;
                var quickSelectItems = productRepository.GetQuickSelectItems(locationID);

                GroupedQuickSelectItems = new ObservableCollection<IGrouping<string, QuickSelectItem>>(
                    quickSelectItems.GroupBy(item => item.CategoryName).OrderBy(group => group.Key)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load Quick Select items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuildCategorySections()
        {
            CategoriesGrid.RowDefinitions.Clear();
            CategoriesGrid.Children.Clear();
            categorySections.Clear();

            int rowIndex = 0;
            bool isAlternate = false; // Used to toggle background colors
            Brush primaryBackground = new SolidColorBrush(Color.FromArgb(34, 0, 0, 0)); // Semi-transparent
            Brush alternateBackground = new SolidColorBrush(Color.FromArgb(34, 50, 50, 50)); // Slightly darker

            foreach (var categoryGroup in GroupedQuickSelectItems)
            {
                // Add header row
                var headerButton = new Button
                {
                    Content = categoryGroup.Key,
                    Style = (System.Windows.Style)FindResource("CategoryHeaderButton"),
                    Background = isAlternate ? alternateBackground : primaryBackground,
                    Tag = categoryGroup.Key
                };

                headerButton.Click += ToggleCategory;
                var headerRow = new RowDefinition { Height = new GridLength(50) };
                CategoriesGrid.RowDefinitions.Add(headerRow);
                CategoriesGrid.Children.Add(headerButton);
                Grid.SetRow(headerButton, rowIndex);

                var contentRow = new RowDefinition { Height = new GridLength(0) };
                CategoriesGrid.RowDefinitions.Add(contentRow);

                var contentWrapPanel = new WrapPanel
                {
                    Margin = new Thickness(10),
                    Visibility = Visibility.Collapsed
                };

                // Calculate the required height based on the number of items
                int itemsPerRow = 5; // Assuming 5 items per row
                double rowHeight = 300; // Fixed height per row
                int totalRows = (int)Math.Ceiling((double)categoryGroup.Count() / itemsPerRow);
                double requiredHeight = totalRows * rowHeight;

                foreach (var item in categoryGroup)
                {

                    var productButton = new Button
                    {
                        Style = (System.Windows.Style)FindResource("quickSelectTileButton"),
                        DataContext = item,
                        Margin = new Thickness(5),
                        Content = new StackPanel
                        {
                            Children =
                    {
                        new TextBlock
                        {
                            Text = item.ProductName,
                            FontFamily = new FontFamily("Inter"),
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(20, 5, 5, 5),
                            Foreground = (Brush)FindResource("merlinWhite_brush")
                        },
                        new TextBlock
                        {
                            Text = item.SKU,
                            FontFamily = new FontFamily("Inter"),
                            FontSize = 16,
                            Margin = new Thickness(20, 0, 5, 5),
                            Foreground = (Brush)FindResource("merlinWhite_brush")
                        },
                        new TextBlock
                        {
                            Text = $"{item.Price:C}",
                            FontFamily = new FontFamily("Inter"),
                            FontSize = 16,
                            Margin = new Thickness(20, 0, 5, 5),
                            Foreground = (Brush)FindResource("merlinWhite_brush")
                        }
                    }
                        },
                        ContextMenu = CreateQuickSelectContextMenu(item) // Attach context menu
                    };

                    productButton.Click += (s, e) =>
                    {
                        var isSelected = productButton.Tag as string == "Selected";

                        if (!isSelected)
                        {
                            productButton.Tag = "Selected";
                            AddToSelectedItems(item);
                        }
                        else
                        {
                            productButton.Tag = null;
                            RemoveFromSelectedItems(item);
                        }
                    };

                    contentWrapPanel.Children.Add(productButton);
                }

                CategoriesGrid.Children.Add(contentWrapPanel);
                Grid.SetRow(contentWrapPanel, rowIndex + 1);

                categorySections[categoryGroup.Key] = (contentRow, contentWrapPanel);
                contentRow.Tag = requiredHeight;

                rowIndex += 2;
                isAlternate = !isAlternate; // Toggle background for categories
            }
        }


        private void ToggleCategory(object sender, RoutedEventArgs e)
        {
            if (sender is Button headerButton && headerButton.Tag is string categoryKey && categorySections.TryGetValue(categoryKey, out var section))
            {
                bool isExpanded = section.row.Height.Value > 0;

                // Get the dynamically calculated height
                double requiredHeight = (double)section.row.Tag;

                // Grid height animation
                var heightAnimation = new GridLengthAnimation
                {
                    From = new GridLength(isExpanded ? requiredHeight : 0),
                    To = new GridLength(isExpanded ? 0 : requiredHeight),
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };

                // Opacity animation for fade-in/out effect
                var opacityAnimation = new DoubleAnimation
                {
                    From = isExpanded ? 1 : 0,
                    To = isExpanded ? 0 : 1,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                // Button movement animation with delay
                double delayIndex = 0.95;
                DoubleAnimation lastOpacityAnimation = null;

                foreach (UIElement child in section.panel.Children)
                {
                    if (child is Button button)
                    {
                        if (!isExpanded)
                        {
                            button.Opacity = 0; // Hide button before animation starts
                        }

                        var transformGroup = new TransformGroup
                        {
                            Children =
                    {
                        new ScaleTransform(),
                        new SkewTransform(),
                        new TranslateTransform()
                    }
                        };
                        button.RenderTransform = transformGroup;

                        var translateTransform = transformGroup.Children.OfType<TranslateTransform>().First();
                        var translateAnimation = new DoubleAnimation
                        {
                            From = isExpanded ? 0 : -20,
                            To = isExpanded ? -20 : 0,
                            Duration = TimeSpan.FromSeconds(0.3),
                            BeginTime = TimeSpan.FromMilliseconds(delayIndex * 92.5),
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };

                        var delayedOpacityAnimation = new DoubleAnimation
                        {
                            From = isExpanded ? 1 : 0,
                            To = isExpanded ? 0 : 1,
                            Duration = TimeSpan.FromSeconds(0.3),
                            BeginTime = TimeSpan.FromMilliseconds(delayIndex * 92.5),
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };

                        translateTransform.BeginAnimation(TranslateTransform.YProperty, translateAnimation);
                        button.BeginAnimation(UIElement.OpacityProperty, delayedOpacityAnimation);

                        lastOpacityAnimation = delayedOpacityAnimation; // Track the last animation
                        delayIndex++;
                    }
                }

                section.panel.Visibility = Visibility.Visible;

                var storyboard = new Storyboard();
                Storyboard.SetTarget(heightAnimation, section.row);
                Storyboard.SetTargetProperty(heightAnimation, new PropertyPath("Height"));
                storyboard.Children.Add(heightAnimation);
                storyboard.Begin();

                if (headerButton.Template.FindName("ArrowIcon", headerButton) is Path arrowIcon &&
                    arrowIcon.RenderTransform is RotateTransform rotateTransform)
                {
                    double targetAngle = isExpanded ? 0 : 180;
                    var rotateAnimation = new DoubleAnimation
                    {
                        To = targetAngle,
                        Duration = TimeSpan.FromSeconds(0.3),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                    };
                    rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                }
            }
        }

        private ContextMenu CreateQuickSelectContextMenu(QuickSelectItem item)
        {
            var contextMenu = new ContextMenu();

            var removeMenuItem = new MenuItem
            {
                Header = "Remove Item",
                Command = new RelayCommand(_ => RemoveQuickSelectItem(item))
            };

            contextMenu.Items.Add(removeMenuItem);

            return contextMenu;
        }
        private void RemoveQuickSelectItem(QuickSelectItem item)
        {
            try
            {
                using (var connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();

                    var query = @"DELETE FROM LocationQuickSelect WHERE LocationID = @LocationID AND SKU = @SKU";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", Properties.Settings.Default.LocationID);
                        command.Parameters.AddWithValue("@SKU", item.SKU);

                        command.ExecuteNonQuery();
                    }
                }

                // Refresh the UI
                LoadQuickSelectItems();
                BuildCategorySections();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove Quick Select item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnQuickSelectButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is QuickSelectItem item)
            {
                var loginWindow = new TransactionLoginWindow();
                if (loginWindow.ShowDialog() == true)
                {
                    // Proceed if the employee is verified
                    var saleWindow = new PointOfSaleWindow_Release(loginWindow.EmployeeID, loginWindow.EmployeeFirstName);

                    // Add the item to the ongoing transaction
                    var inventoryItem = new InventoryItem
                    {
                        SKU = item.SKU,
                        ProductName = item.ProductName,
                        QuantityOnHandSellable = item.QuantityOnHandSellable,
                        Price = item.Price,
                        CategoryID = item.CategoryID
                    };

                    saleWindow.AddProductToSale(inventoryItem); // Ensure this method is defined
                    saleWindow.ShowDialog();
                }
                else
                {
                    //
                }
            }
        }

        private ObservableCollection<QuickSelectItem> selectedItems = new ObservableCollection<QuickSelectItem>();

        private void AddToSelectedItems(QuickSelectItem item)
        {
            if (!selectedItems.Contains(item))
                selectedItems.Add(item);
        }

        private void RemoveFromSelectedItems(QuickSelectItem item)
        {
            if (selectedItems.Contains(item))
                selectedItems.Remove(item);
        }

        private void OnProcessSelectedItems_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedItems.Any())
            {
                MessageBox.Show("No items selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var loginWindow = new TransactionLoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                var saleWindow = new PointOfSaleWindow_Release(loginWindow.EmployeeID, loginWindow.EmployeeFirstName);

                foreach (var item in selectedItems)
                {
                    var inventoryItem = new InventoryItem
                    {
                        SKU = item.SKU,
                        ProductName = item.ProductName,
                        QuantityOnHandSellable = item.QuantityOnHandSellable,
                        Price = item.Price,
                        CategoryID = item.CategoryID
                    };

                    saleWindow.AddProductToSale(inventoryItem);
                }

                saleWindow.ShowDialog();
                selectedItems.Clear(); // Clear selection after processing
            }
            else
            {
                return;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
