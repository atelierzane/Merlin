using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Helpers
{

    public class TransactionHelper
    {
        const decimal taxRate = 0.75m;

        public decimal GetTaxRate()
        { return taxRate; }
    }
}
