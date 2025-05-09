using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class QuickSelectItem : InventoryItem
    {
        public string ProductDisplayText { get; set; }
    }

}
