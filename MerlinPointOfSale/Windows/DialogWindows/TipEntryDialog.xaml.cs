using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    public partial class TipEntryDialog : Window
    {
        public decimal TipAmount { get; private set; } = 0m;

        public TipEntryDialog()
        {
            InitializeComponent();
            TipAmountTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(TipAmountTextBox.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal tip))
            {
                if (tip >= 0)
                {
                    TipAmount = tip;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Tip amount cannot be negative.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid tip amount.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TipAmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numeric input and one decimal point
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]*\.?[0-9]*$");
        }
    }
}
