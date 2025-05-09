using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Models
{
    // Location class representing data from the database
    public class Location
    {
        public string LocationID { get; set; }
        public string LocationStreetAddress { get; set; }
        public string LocationCity { get; set; }
        public string LocationState { get; set; }
        public string LocationZIP { get; set; }
        public string LocationPhoneNumber { get; set; }
        public string LocationManagerID { get; set; }
        public string LocationType { get; set; }
        public bool LocationIsTradeHold { get; set; }
        public int LocationTradeHoldDuration { get; set; }
    }
}
