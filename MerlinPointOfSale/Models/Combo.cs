using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class Combo
    {
        public string ComboSKU { get; set; }
        public string ComboName { get; set; }
        public decimal ComboPrice { get; set; }
        public List<ComboItem> Items { get; set; } = new List<ComboItem>();

        public IEnumerable<SummaryItem> GenerateSummaryItems()
        {
            return Items.Select(item => new SummaryItem
            {
                SKU = item.SKU,
                ProductName = item.ProductName,
                CategoryID = item.CategoryID,
                Quantity = item.Quantity,
                Price = item.Price,
                ComboSKU = this.ComboSKU, // Reference back to the combo
                IsCombo = true,
                ComboDetails = this
            });
        }
    }
}
