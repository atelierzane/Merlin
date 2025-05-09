using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    // Vendor class representing data from the database
    public class Vendor
    {
        public string VendorID { get; set; }
        public string VendorName { get; set; }
        public string VendorContact { get; set; }
        public string VendorContactPhone { get; set; }
        public string VendorContactEmail { get; set; }
        public string VendorSalesRep { get; set; }
        public string VendorSalesRepPhone { get; set; }
        public string VendorSalesRepEmail { get; set; }
    }
}
