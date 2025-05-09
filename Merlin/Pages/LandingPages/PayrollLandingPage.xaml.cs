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
using MerlinAdministrator.Pages.PayrollPages;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for PayrollLandingPage.xaml
    /// </summary>
    public partial class PayrollLandingPage : Page
    {
        public PayrollLandingPage()
        {
            InitializeComponent();
        }

        private void OnAllocateHours_Click(object sender, RoutedEventArgs e)
        {
            payrollFrame.Content = new AllocateHoursPage();
        }
    }
}
