using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MerlinBackOffice.Windows.InventoryWindows;

namespace MerlinBackOffice.Menus
{
    /// <summary>
    /// Interaction logic for InventoryAdjustmentsMenu.xaml
    /// </summary>
    public partial class InventoryAdjustmentsMenu : Window
    {
        public InventoryAdjustmentsMenu()
        {
            InitializeComponent();
        }

        private void OnSellableAdjustments_Click(object sender, RoutedEventArgs e)
        {
            SellableAdjustmentWindow sellableAdjustmentWindow = new SellableAdjustmentWindow();
            sellableAdjustmentWindow.ShowDialog();
        }

        private void OnDefectiveAdjustments_Click(object sender, RoutedEventArgs e)
        {
            DefectiveAdjustmentWindow defectiveAdjustmentWindow = new DefectiveAdjustmentWindow();
            defectiveAdjustmentWindow.ShowDialog();
        }
    }
}
