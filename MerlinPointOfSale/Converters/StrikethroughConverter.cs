using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace MerlinPointOfSale.Converters
{
    public class StrikethroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is decimal adjustedValue))
            {
                return null; // No strikethrough if null or no discount applied
            }

            decimal originalValue = (decimal)parameter;
            if (adjustedValue < originalValue)
            {
                return TextDecorations.Strikethrough; // Apply strikethrough if discount is applied
            }

            return null; // No strikethrough if no discount
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
