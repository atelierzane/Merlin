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

using MerlinAdministrator.Pages.DepartmentManagerPages;
namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for DepartmentManagerLandingPage.xaml
    /// </summary>
    public partial class DepartmentManagerLandingPage : Page
    {
        public DepartmentManagerLandingPage()
        {
            InitializeComponent();
        }

        private void OnBtnAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            departmentManagerFrame.Content = new AddDepartmentPage();
        }

        private void OnBtnEditDepartment_Click(object sender, RoutedEventArgs e)
        {
            departmentManagerFrame.Content = new EditDepartmentPage();
        }

        private void OnBtnViewDepartments_Click(object sender, RoutedEventArgs e)
        {
            departmentManagerFrame.Content = new ViewDepartmentsPage();
        }
    }
}
