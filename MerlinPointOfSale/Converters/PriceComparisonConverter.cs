using MerlinPointOfSale.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MerlinPointOfSale.Converters
{
    public class PriceComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is SummaryItem item))
            {
                return null; // If null, return null to avoid errors
            }

            // Apply strikethrough only if AdjustedValue is lower than the original Value
            if (item.AdjustedValue < item.Value && targetType == typeof(TextDecorationCollection))
            {
                return TextDecorations.Strikethrough; // Strikethrough original price if discount is applied
            }

            // Always show original price, but return null for no strikethrough if no discount
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
