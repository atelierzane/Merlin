using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    // Model class to represent a Carton
    public class Carton
    {
        public string CartonID { get; set; }
        public string CartonOrigin { get; set; }
        public string CartonDestination { get; set; }
        public string CartonStatus { get; set; }
        public int TotalItemsShipped { get; set; }
        public bool IsSelected { get; set; }
    }
}
