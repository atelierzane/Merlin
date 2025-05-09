using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
   public class Transaction
    {
        public string TransactionId { get; set; }
        public int TransactionNumber { get; set; }
        public string RegisterNumber { get; set; }
        public string LocationID { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime TransactionTime { get; set; }
        public string EmployeeID { get; set; }
        public string? CustomerID { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discounts { get; set; }
        public string PaymentMethod { get; set; }
        public decimal NetCash { get; set; }
        public bool IsSuspended { get; set; }

        public bool IsPostVoid { get; set; }

        public List<TransactionItems> TransactionItems { get; set; } = new List<TransactionItems>();
    }
}
