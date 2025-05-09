using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class TransactionItems : Product
    {
        public int Quantity { get; set; }

        public string Description { get; set; }

        public int TransactionNumber { get; set; }  
    }
}
