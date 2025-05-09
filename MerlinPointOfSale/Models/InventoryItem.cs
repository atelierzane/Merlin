using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class InventoryItem : Product
    {
        public string LocationID { get; set; }
        public string ProductSerialNumber { get; set; }
        public int QuantityOnHandSellable { get; set; }
        public int QuantityOnHandDefective { get; set; }
        public bool IsPartOfCombo { get; set; }
        public int Quantity { get; set; }


    }
}
