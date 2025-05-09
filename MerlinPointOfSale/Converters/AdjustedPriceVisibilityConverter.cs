using MerlinPointOfSale.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MerlinPointOfSale.Converters
{
    public class AdjustedPriceVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is SummaryItem item))
            {
                return Visibility.Collapsed; // Collapse if null
            }

            // Show only if AdjustedValue is less than the original Value
            if (item.AdjustedValue < item.Value)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed; // Collapse if no discount
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
