using MerlinPointOfSale.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    public partial class GiftReceiptItemSelectionDialogReprint : Window
    {
        public List<TransactionItems> SelectedItems { get; private set; }

        public GiftReceiptItemSelectionDialogReprint(List<TransactionItems> items)
        {
            InitializeComponent();
            if (items == null || !items.Any())
            {
                MessageBox.Show("No items available for this transaction.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = false;
                Close();
                return;
            }
            ItemList.ItemsSource = items;
            SelectedItems = new List<TransactionItems>();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedItems = ItemList.SelectedItems.Cast<TransactionItems>().ToList();
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
