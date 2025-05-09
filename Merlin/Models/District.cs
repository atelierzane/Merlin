using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class District
    {
        public string DistrictID { get; set; }
        public string DistrictName { get; set; }

        public string DistrictSupervisorID { get; set; }

        public string RegionID { get; set; }
        public string MarketID { get; set; }
        public string DivisionID {  get; set; }
    }
}
