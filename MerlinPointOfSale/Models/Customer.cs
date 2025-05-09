using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
   public class Customer
    {
        public string CustomerID { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerStreetAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerZIP {  get; set; }
        public bool CustomerLoyalty { get; set; }
        public bool CustomerLoyaltyPaid { get; set; }
        public decimal CustomerStoreCredit { get; set; }
        public int CustomerPoints { get; set; }
        public DateTime CustomerLoyaltyExpiration { get; set; }
    }
}
