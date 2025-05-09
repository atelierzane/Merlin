using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class TradeItem : InventoryItem
    {
        public bool IsDefectiveTrade { get; set; }
    }
}
