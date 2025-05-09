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

using MerlinPointOfSale.Windows;
using MerlinPointOfSale.Windows.DialogWindows;

namespace MerlinPointOfSale
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void onBtnConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationWindow configurationWindow = new ConfigurationWindow();
            configurationWindow.ShowDialog();
        }

        private void onBtnStartTransaction_Click(object sender, RoutedEventArgs e)
        {
            TransactionLoginWindow verificationWindow = new TransactionLoginWindow();
            bool? dialogResult = verificationWindow.ShowDialog();

            if (dialogResult == true)
            {
                // Assuming EmployeeNumber and EmployeeFirstName are already fetched in the TransactionLoginWindow
                string employeeID = verificationWindow.EmployeeID;
                string employeeFirstName = verificationWindow.EmployeeFirstName;

                // Check if employee details are successfully retrieved
                if (!string.IsNullOrEmpty(employeeID) && !string.IsNullOrEmpty(employeeFirstName))
                {
                    PointOfSaleWindow pointOfSaleWindow = new PointOfSaleWindow(employeeID, employeeFirstName);
                    pointOfSaleWindow.ShowDialog();
                }
                else
                {
                    //MessageBox.Show("Employee details not found.");
                }
            }
            else
            {
                // MessageBox.Show("Access denied. Please verify your credentials.");
            }
        }
    }
}
