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

using MerlinAdministrator.Pages.PromotionManagerPages;

namespace MerlinAdministrator.Pages.LandingPages
{
    /// <summary>
    /// Interaction logic for PromotionManagerLandingPage.xaml
    /// </summary>
    public partial class PromotionManagerLandingPage : Page
    {
        public PromotionManagerLandingPage()
        {
            InitializeComponent();
        }

        private void BtnAddCombo_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content = new AddComboPage();
        }

        private void BtnComboSearch_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content = new ComboSearchPage();
        }

        private void BtnEditCombo_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content = new EditComboPage();
        }

        private void BtnRemoveCombo_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content = new RemoveComboPage();
        }

        private void BtnRemoveComboBulk_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content = new RemoveComboBulkPage();
        }

        private void BtnAddPromotion_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content = new AddPromotionPage();
        }

        private void BtnSearchPromotions_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content = new PromotionSearchPage();
        }

        private void BtnEditPromotions_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content= new EditPromotionPage();
        }

        private void BtnRemovePromotions_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content= new RemovePromotionPage();
        }

        private void BtnRemovePromotionsBulk_Click(object sender, RoutedEventArgs e)
        {
            promotionManagerFrame.Content= new RemovePromotionBulkPage();
        }
    }
}
