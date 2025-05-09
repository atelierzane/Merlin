using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{

    // CartonItem class to represent an item in the carton
    public class CartonItem : Product
    {
        public string ProductSerialNumber { get; set; }
        public int ProductQuantityShipped { get; set; }
    }
}
