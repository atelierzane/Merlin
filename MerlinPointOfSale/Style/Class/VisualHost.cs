using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace MerlinPointOfSale.Style.Class
{
    public class VisualHost : FrameworkElement
    {
        private List<Visual> visuals = new List<Visual>();

        protected override int VisualChildrenCount => visuals.Count;
        protected override Visual GetVisualChild(int index) => visuals[index];

        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);
            AddVisualChild(visual);
            AddLogicalChild(visual);
        }

        public void ClearVisuals()
        {
            while (visuals.Any())
            {
                var visual = visuals.Last();
                RemoveVisualChild(visual);
                RemoveLogicalChild(visual);
                visuals.Remove(visual);
            }
        }
    }
}
