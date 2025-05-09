using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinBackOffice.Models
{
    public class Count
    {
        public string? CountID { get; set; } // Primary key for the count
        public string? CountType { get; set; } // Type of the count (e.g., "Category Count", "Defective Count")
        public string? CountCategory { get; set; } // Category associated with the count (if applicable)
        public DateTime? CompletedDate { get; set; } // Date when the count was completed
        public string? CompletedByEmployee { get; set; } // Name of the employee who completed the count
        public decimal? Accuracy { get; set; } // Accuracy of the count as a percentage
    }
}

