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
using MerlinBackOffice.Pages;

namespace MerlinBackOffice.Windows
{
    /// <summary>
    /// Interaction logic for HumanResourcesWindow.xaml
    /// </summary>
    public partial class HumanResourcesWindow : Window
    {
        public HumanResourcesWindow()
        {
            InitializeComponent();
        }

        private void OnSearch_Click(object sender, RoutedEventArgs e)
        {
            HumanResourcesFrame.Content = new EmployeeSearchPage();
        }

        private void OnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            HumanResourcesFrame.Content = new AddEmployeePage();
        }

        private void OnEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            HumanResourcesFrame.Content = new EditEmployeePage();
        }

        private void OnRemoveEmployee_Click(object sender, RoutedEventArgs e)
        {
            HumanResourcesFrame.Content = new RemoveEmployeePage();
        }
    }
}
