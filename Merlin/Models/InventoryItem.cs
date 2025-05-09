using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class InventoryItem : Product
    {
        public string LocationID { get; set; }
        public string ProductSerialNumber { get; set; }
        public int QuantityOnHandSellable { get; set; }
        public int QuantityOnHandDefective { get; set; }
    }
}
