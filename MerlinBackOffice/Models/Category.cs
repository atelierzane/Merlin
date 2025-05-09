using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinBackOffice.Models
{
    public class Category
    {
        public string CategoryID { get; set; }
        public string CategoryName { get; set; }

        public List<CategoryTrait> Traits { get; set; } = new List<CategoryTrait>();

    }
}
