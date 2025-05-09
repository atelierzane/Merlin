using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinBackOffice.Models
{
    public class AdjustmentItem
    {
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string CategoryID { get; set; }
        public string AdjustmentType { get; set; }
        public int OriginalQuantity { get; set; }
        public int NewQuantity { get; set; }
        public int QuantityDifference { get; set; }
    }
}
