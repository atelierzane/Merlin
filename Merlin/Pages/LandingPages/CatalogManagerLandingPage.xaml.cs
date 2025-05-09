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

using MerlinAdministrator.Pages.CatalogManagerPages;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for CatalogManagerLandingPage.xaml
    /// </summary>
    public partial class CatalogManagerLandingPage : Page
    {
        public CatalogManagerLandingPage()
        {
            InitializeComponent();
        }

        private void OnBtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            catalogManagerFrame.Content = new AddProductPage();
        }

        private void OnBtnSearch_Click(object sender, RoutedEventArgs e)
        {
            catalogManagerFrame.Content = new CatalogSearchPage();
        }

        private void OnBtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            catalogManagerFrame.Content = new EditProductPage();
        }

        private void OnBtnRemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            catalogManagerFrame.Content = new RemoveProductPage();
        }

        private void OnBtnRemoveProductBulk_Click(object sender, RoutedEventArgs e)
        {
            catalogManagerFrame.Content = new RemoveProductBulkPage();
        }

        private void OnBtnEditProductBulk_Click(object sender, RoutedEventArgs e)
        {
            catalogManagerFrame.Content = new EditProductBulkPage();
        }
    }
}
