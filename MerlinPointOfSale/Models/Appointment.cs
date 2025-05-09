using System;
using System.Collections.Generic;
using System.Linq;

namespace MerlinPointOfSale.Models
{
    public class Appointment
    {
        public string AppointmentID { get; set; }
        public string LocationID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; } // Full name of the employee
        public List<Service> Services { get; set; } = new List<Service>();
        public List<ServiceAddOn> ServiceAddOns { get; set; } = new List<ServiceAddOn>();
        public List<ServiceFee> ServiceFees { get; set; } = new List<ServiceFee>();

        // Computed properties for display
        public string ClientName => $"{CustomerFirstName} {CustomerLastName}".Trim();
        public string ServicesSummary => Services.Any() ? string.Join(", ", Services.Select(s => s.ServiceName)) : "None";
        public string AddOnsSummary => ServiceAddOns.Any() ? string.Join(", ", ServiceAddOns.Select(a => a.ServicePlusName)) : "None";
        public string FeesSummary => ServiceFees.Any() ? string.Join(", ", ServiceFees.Select(f => f.ServiceFeeName)) : "None";
        public string DisplayTime => DateTime.Today.Add(AppointmentTime).ToString("hh:mm tt");


    }
}
