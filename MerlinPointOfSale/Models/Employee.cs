using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class Employee
    {
        public string EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmployeeType { get; set; }
        public string EmployeeTransactionID { get; set; }
        public string EmployeeInitials { get; set; }
        public string EmployeePassword { get; set; }
        public bool? EmployeeIsCommissionEligible { get; set; }
        public decimal? EmployeeCommissionRate { get; set; }
        public bool? EmployeeIsTipEligible { get; set; }
    }
}
