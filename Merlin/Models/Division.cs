using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class Division
    {
        public string DivisionID { get; set; }
        public string DivisionName { get; set; }

        public string DivisionSupervisorID { get; set; }

        public string MarketID { get; set; }
    }
}
