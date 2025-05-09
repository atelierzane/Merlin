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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MerlinAdministrator.Pages;
using MerlinAdministrator.Pages.KPIManager;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for KPIManagerLandingPage.xaml
    /// </summary>
    public partial class KPIManagerLandingPage : Page
    {
        public KPIManagerLandingPage()
        {
            InitializeComponent();
        }

        private void OnAddKPI_Click(object sender, RoutedEventArgs e)
        {
            kpiManagerFrame.Content = new AddKPIPage();
        }

        private void OnSearch_Click(object sender, RoutedEventArgs e)
        {
            kpiManagerFrame.Content = new KPISearchPage();
        }
    }
}
