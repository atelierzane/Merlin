using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MerlinPointOfSale.Converters
{
    public class PercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                return GetColorForPercentage(percentage);
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private Brush GetColorForPercentage(double percentage)
        {
            // Define more subdued colors
            Color darkRed = Color.FromRgb(235, 0, 0);   // Dark Red
            Color darkYellow = Color.FromRgb(255, 204, 0); // Dark Yellow
            Color darkGreen = Color.FromRgb(0, 200, 0); // Dark Green

            byte r = 0, g = 0, b = 0;

            if (percentage <= 0.5)
            {
                // Dark Red to Dark Yellow
                r = darkRed.R;
                g = (byte)(darkRed.G + (darkYellow.G - darkRed.G) * (percentage * 2));
                b = darkRed.B;
            }
            else
            {
                // Dark Yellow to Dark Green
                r = (byte)(darkYellow.R + (darkGreen.R - darkYellow.R) * ((percentage - 0.5) * 2));
                g = (byte)(darkYellow.G + (darkGreen.G - darkYellow.G) * ((percentage - 0.5) * 2));
                b = darkYellow.B;
            }

            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
    }
}
