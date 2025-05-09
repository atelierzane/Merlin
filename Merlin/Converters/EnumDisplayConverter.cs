using MerlinAdministrator.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MerlinAdministrator.Converters
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PredefinedTrait trait)
            {
                // Use a switch statement to convert enum to a user-friendly string
                switch (trait)
                {
                    case PredefinedTrait.Serialized:
                        return "Serialized";
                    case PredefinedTrait.WarrantyEligible:
                        return "Warranty Eligible";
                    case PredefinedTrait.ReturnEligible:
                        return "Return Eligible";
                    case PredefinedTrait.TradeEligible:
                        return "Trade Eligible";
                    case PredefinedTrait.AgeRestricted:
                        return "Age Restricted";
                    default:
                        return trait.ToString(); // Fallback to default
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
