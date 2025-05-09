using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    // Model class to represent Carton Details
    public class CartonDetail : InventoryItem
    {
        
        public int ProductQuantityShipped { get; set; }
        public int ProductQuantityReceived { get; set; }
    }
}
