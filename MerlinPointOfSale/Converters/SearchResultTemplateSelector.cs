using MerlinPointOfSale.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace MerlinPointOfSale.Converters
{
    public class SearchResultTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ProductTemplate { get; set; }
        public DataTemplate ComboTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Product)
                return ProductTemplate;
            if (item is Combo)
                return ComboTemplate;

            return base.SelectTemplate(item, container);
        }
    }

}
