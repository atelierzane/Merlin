using MerlinPointOfSale.Pages.ReleasePointOfSalePages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MerlinPointOfSale.Pages.ReleaseLandingPages
{
    /// <summary>
    /// Interaction logic for CustomersLandingPage.xaml
    /// </summary>
    public partial class PointOfSaleLandingPage : Page
    {
        public PointOfSaleLandingPage()
        {
            InitializeComponent();

            // Set menu items for the MenuBarControl
            MenuBar.MenuItems = new ObservableCollection<string>
            {
                "Point of Sale",
                "Quick Select",

            };

            MenuBar.ButtonClicked += MenuBar_ButtonClicked;


        }

        private void MenuBar_ButtonClicked(object sender, string buttonName)
        {
            switch (buttonName)
            {
                case "Point of Sale":
                    NavigateToPage(new RegisterFunctionsPage());
                    break;

                case "Quick Select":
                    NavigateToPage(new QuickSelectPage()); // Use singleton instance
                    break;
            }
        }


        private void NavigateToPage(Page page)
        {
            var fadeOutAnimation = (Storyboard)FindResource("SubPageTransitionOut");
            fadeOutAnimation.Completed += (s, _) =>
            {
                PointOfSaleFrame.Content = page;
                var fadeInAnimation = (Storyboard)FindResource("SubPageTransitionIn");
                fadeInAnimation.Begin(PointOfSaleFrame);
            };
            fadeOutAnimation.Begin(PointOfSaleFrame);
        }
    }
}
