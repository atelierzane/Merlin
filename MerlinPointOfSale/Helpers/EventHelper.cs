using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale.Helpers
{
    public class EventHelper
    {
        private static EventHelper instance;

        public static EventHelper Instance => instance ??= new EventHelper();

        public EventHelper() { }

        public event Action QuickSelectUpdated;

        public void RaiseQuickSelectUpdated()
        {
            QuickSelectUpdated?.Invoke();
        }
    }
}
