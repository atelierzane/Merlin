using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class RefundResponse
    {
        public string ChargeId { get; set; }
        public string RefundId { get; set; }
        public long Amount { get; set; }
        public string Status { get; set; }
    }
}
