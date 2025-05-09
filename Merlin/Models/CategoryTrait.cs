using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public enum PredefinedTrait
    {
        Serialized,
        WarrantyEligible,
        ReturnEligible,
        TradeEligible,
        AgeRestricted
    }

    public class CategoryTrait
    {
        public PredefinedTrait Trait { get; set; } // Selected from the predefined list
        public bool IsRequired { get; set; }       // true if the trait is mandatory
    }
}
