using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class SuspendedTransaction
    {
        public string TransactionID { get; set; }
        public int TransactionNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public TimeSpan TransactionTime { get; set; }
        public string EmployeeID { get; set; }
    }

}
