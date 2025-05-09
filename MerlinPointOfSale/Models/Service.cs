using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class Service
    {
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }
        public decimal ServicePrice { get; set; }
        public string ServicePricedBy { get; set; }
        public bool IsServicePlus { get; set; }
        public bool HasServiceFees { get; set; }
    }
}

