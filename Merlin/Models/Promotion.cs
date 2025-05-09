using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class Promotion
    {
        public string PromotionID { get; set; }
        public string PromotionName { get; set; }
        public DateTime PromotionStartDate { get; set; }
        public DateTime PromotionEndDate { get; set; }
        public decimal? PromotionDiscountValue { get; set; }
        public string PromotionType { get; set; }
        public bool IsSelected { get; set; }
    }
}
