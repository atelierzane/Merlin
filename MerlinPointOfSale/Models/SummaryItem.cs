using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    public class SummaryItem : InventoryItem
    {
        public decimal Value { get; set; }
        public decimal AdjustedValue { get; set; }
        public string Type { get; set; } // Sale, Trade, or Return
        public int SummaryQuantity { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal ManualDiscountedValue { get; set; }
        public decimal LoyaltyDiscountedValue { get; set; }

        public bool IsDefective { get; set; }
        public decimal DisplayValue => AdjustedValue != 0 ? AdjustedValue : Value;

        // Added properties for combo tracking
        public bool IsLoyaltyDiscount { get; set; }
        public bool IsManualDiscount { get; set; }
        public bool IsCombo { get; set; }
        public Combo ComboDetails { get; set; } // Holds the combo details
        public string ComboSKU { get; set; } // Holds the SKU of the combo if this item is part of a combo

        // New property for tracking promotions applied
        public string PromotionsApplied { get; set; }
    }


}

