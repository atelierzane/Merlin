using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class Product
    {
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string CategoryID { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }

        public decimal TradeValue { get; set; }
        public string SupplierID { get; set; }
        public bool IsSelected { get; set; }
        public bool IsBaseSKU { get; set; }
    }
}
