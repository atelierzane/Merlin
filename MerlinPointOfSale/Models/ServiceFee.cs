using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class ServiceFee
    {
        public string ServiceFeeID { get; set; }
        public string ServiceFeeName { get; set; }
        public decimal ServiceFeePrice { get; set; }
        public string ServiceFeePricedBy { get; set; }
    }
}

