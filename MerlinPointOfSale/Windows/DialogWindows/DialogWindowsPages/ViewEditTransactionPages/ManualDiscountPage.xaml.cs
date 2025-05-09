using System.Windows;
using System.Windows.Controls;
using MerlinPointOfSale.Models;
using System.Collections.Generic;

namespace MerlinPointOfSale.Windows.DialogWindows.DialogWindowsPages.ViewEditTransactionPages
{
    public partial class ManualDiscountPage : Page
    {
        private List<SummaryItem> _transactionItems;

        public ManualDiscountPage(List<SummaryItem> transactionItems)
        {
            InitializeComponent();
            _transactionItems = transactionItems;

            // Bind the transaction items to the DataGrid
            dgTransactionItems.ItemsSource = _transactionItems;
        }

        private void OnApplyDiscount_Click(object sender, RoutedEventArgs e)
        {
            if (dgTransactionItems.SelectedItem is SummaryItem selectedItem)
            {
                decimal discountAmount;

                // Parse the discount amount entered by the user
                if (decimal.TryParse(txtDiscountAmount.Text, out discountAmount))
                {
                    string selectedDiscountType = (cbDiscountType.SelectedItem as ComboBoxItem)?.Content.ToString();

                    if (selectedDiscountType == "Percentage")
                    {
                        if (discountAmount >= 0 && discountAmount <= 100)
                        {
                            ApplyPercentageDiscount(selectedItem, discountAmount);
                        }
                        else
                        {
                            MessageBox.Show("Invalid percentage. Please enter a value between 0 and 100.");
                        }
                    }
                    else
                    {
                        ApplyDollarAmountDiscount(selectedItem, discountAmount);
                    }

                    // After applying the discount, notify the parent window to refresh
                    dgTransactionItems.Items.Refresh();
                }
                else
                {
                    MessageBox.Show("Invalid discount amount.");
                }
            }
            else
            {
                MessageBox.Show("Please select an item to apply the discount.");
            }
        }

        // Apply a percentage discount to the selected item
        private void ApplyPercentageDiscount(SummaryItem item, decimal percentage)
        {
            item.ManualDiscountedValue = item.Value * (percentage / 100);
            item.AdjustedValue = item.Value - item.ManualDiscountedValue;
            item.IsManualDiscount = true;
        }

        // Apply a dollar amount discount to the selected item
        private void ApplyDollarAmountDiscount(SummaryItem item, decimal amount)
        {
            item.ManualDiscountedValue = amount;
            item.AdjustedValue = item.Value - item.ManualDiscountedValue;
            item.IsManualDiscount = true;
        }
    }
}
