using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinBackOffice.Models
{
    public class Employee
    {
        public string EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmployeeType { get; set; }
        public bool EmployeeOvertimeEligible { get; set; }
        public bool EmployeeComissionEligible { get; set; }
        public decimal EmployeeCommissionRate { get; set; }
        public decimal EmployeeComissionLimit {  get; set; }
    }
}
