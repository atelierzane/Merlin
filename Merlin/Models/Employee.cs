using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class Employee
    {
        public string EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmployeeType { get; set; }
        public string DisplayName => $"{FirstName} {LastName} (ID: {EmployeeID})";

        public override string ToString()
        {
            return DisplayName;
        }
    }
}

