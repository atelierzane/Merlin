using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinBackOffice.Models
{
    public class CountItem : InventoryItem
    {
        public int ExpectedQuantity { get; set; } // Quantity expected during the count
        public int ScannedQuantity { get; set; } // Quantity scanned during the count
        public int Discrepancy => ScannedQuantity - ExpectedQuantity; // Difference between scanned and expected quantities
        public bool IsDiscrepancy => Discrepancy != 0; // Indicates if there is a discrepancy
    }
}
