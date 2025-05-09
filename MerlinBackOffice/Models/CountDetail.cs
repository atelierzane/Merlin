using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinBackOffice.Models
{
    public class CountDetail
    {
        public string CountID { get; set; } // Foreign key linking to the Count table
        public string SKU { get; set; } // SKU of the product being counted
        public int SKUQuantityExpected { get; set; } // Expected quantity of the SKU
        public int SKUQuantityActual { get; set; } // Actual/scanned quantity of the SKU
        public int SKUDiscrepancy { get; set; } // Difference between expected and actual quantities
    }
}
