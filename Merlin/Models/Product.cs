using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class Product
    {
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string UPC { get; set; }
        public decimal Price { get; set; }

        public decimal Cost { get; set; }
        public string VendorID { get; set; }
        public bool IsSelected { get; set; }
    }
}
