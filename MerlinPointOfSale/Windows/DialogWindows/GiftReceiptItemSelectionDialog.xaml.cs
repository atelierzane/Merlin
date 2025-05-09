using MerlinPointOfSale.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    /// <summary>
    /// Interaction logic for GiftReceiptItemSelectionDialog.xaml
    /// </summary>
    public partial class GiftReceiptItemSelectionDialog : Window
    {
        public List<SummaryItem> SelectedItems { get; private set; }

        public GiftReceiptItemSelectionDialog(List<SummaryItem> items)
        {
            InitializeComponent();
            ItemList.ItemsSource = items;
            SelectedItems = new List<SummaryItem>();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedItems = ItemList.SelectedItems.Cast<SummaryItem>().ToList();
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }

}
