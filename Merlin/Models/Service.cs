using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class Service

    {
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }

        public override string ToString()
        {
            return ServiceName; 
        }

    }
}
