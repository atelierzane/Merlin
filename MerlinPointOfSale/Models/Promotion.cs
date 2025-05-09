using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class Promotion
    {
        public string PromotionID { get; set; }
        public string PromotionName { get; set; }
        public DateTime PromotionStartDate { get; set; }
        public DateTime PromotionEndDate { get; set; }
        public decimal PromotionDiscountValue { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsDollarValue { get; set; }
        public bool IsCodeActivated { get; set; }
        public string ActivationCode { get; set; }
        public List<string> ApplicableCategories { get; set; } = new List<string>();
        public List<string> ApplicableSKUs { get; set; } = new List<string>();

        public bool IsApplicableToCategory(string categoryID)
        {
            return !string.IsNullOrEmpty(categoryID) &&
                   ApplicableCategories.Any(c => string.Equals(c.Trim(), categoryID.Trim(), StringComparison.OrdinalIgnoreCase));
        }


        public bool IsApplicableToSKU(string sku)
        {
            return !string.IsNullOrEmpty(sku) &&
                   ApplicableSKUs.Contains(sku);
        }
    }
}
