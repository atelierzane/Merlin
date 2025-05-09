using MerlinPointOfSale.Models;
using MerlinPointOfSale.Repositories;
using System.Collections.ObjectModel;
using System.Windows;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    public partial class ProductSelectionWindow : Window
    {
        private readonly ProductRepository productRepository;
        public ObservableCollection<Product> AvailableProducts { get; private set; }
        public Product SelectedProduct { get; private set; }

        public ProductSelectionWindow(string categoryID, string categoryName, ProductRepository productRepository)
        {
            InitializeComponent();
            this.productRepository = productRepository;

            // Set the category name in the UI
            txtCategoryName.Text = categoryName;

            // Load products from the category
            AvailableProducts = new ObservableCollection<Product>(productRepository.GetProductsByCategory(categoryID));

            // Handle cases where no products are found for the category
            if (AvailableProducts.Count == 0)
            {
                MessageBox.Show("No products found for this category.", "No Products", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Bind the product list to the ListView
            lvProducts.ItemsSource = AvailableProducts;
        }



        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            // Set the selected product
            if (lvProducts.SelectedItem != null)
            {
                SelectedProduct = (Product)lvProducts.SelectedItem;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a product before confirming.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
