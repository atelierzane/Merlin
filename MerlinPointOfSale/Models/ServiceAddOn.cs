using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class ServiceAddOn
    {
        public string ServicePlusID { get; set; }
        public string ServicePlusName { get; set; }
        public decimal ServicePlusPrice { get; set; }
        public string ServicePlusPricedBy { get; set; }
    }
}

