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

using MerlinAdministrator.Pages.EmployeeManagerPages;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for EmployeeManagerLandingPage.xaml
    /// </summary>
    public partial class EmployeeManagerLandingPage : Page
    {
        public EmployeeManagerLandingPage()
        {
            InitializeComponent();
        }

        private void OnBtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            employeeManagerFrame.Content = new AddEmployeePage();
        }

        private void OnBtnSearch_Click(object sender, RoutedEventArgs e)
        {
            employeeManagerFrame.Content = new EmployeeSearchPage();
        }

        private void OnBtnEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            employeeManagerFrame.Content = new EditEmployeePage();
        }

        private void OnBtnRemoveEmployee_Click(object sender, RoutedEventArgs e)
        {
            employeeManagerFrame.Content = new RemoveEmployeePage();
        }
    }
}
