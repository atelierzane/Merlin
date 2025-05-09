using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class Region
    {
        public string RegionID { get; set; }
        public string RegionName { get; set; }

        public string RegionSupervisorID { get; set; }

        public string MarketID { get; set; }
        public string DivisionID { get; set; }
    }
}


