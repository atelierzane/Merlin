using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinAdministrator.Models
{
    public class DivisionNode
    {
        public Division Division { get; set; }
        public List<MarketNode> Markets { get; set; } = new();
    }

    public class MarketNode
    {
        public Market Market { get; set; }
        public List<RegionNode> Regions { get; set; } = new();
    }

    public class RegionNode
    {
        public Region Region { get; set; }
        public List<DistrictNode> Districts { get; set; } = new();
    }

    public class DistrictNode
    {
        public District District { get; set; }
        public List<Location> Locations { get; set; } = new();
    }
}
