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
    public partial class CustomersLandingPage : Page
    {
        public CustomersLandingPage()
        {
            InitializeComponent();

            // Set menu items for the MenuBarControl
            MenuBar.MenuItems = new ObservableCollection<string>
            {
                "Details",
                "Transaction History",
                "Loyalty + Subscriptions",
            };

            MenuBar.ButtonClicked += MenuBar_ButtonClicked;


        }

        private void MenuBar_ButtonClicked(object sender, string buttonName)
        {
            switch (buttonName)
            {
                case "Details":
                    
                    break;

                case "Transaction History":

                    break;

                case "Loyalty + Subscriptions":

                    break;

            }
        }

        private void NavigateToPage(Page page)
        {
            var fadeOutAnimation = (Storyboard)FindResource("SubPageTransitionOut");
            fadeOutAnimation.Completed += (s, _) =>
            {
                CustomersFrame.Content = page;
                var fadeInAnimation = (Storyboard)FindResource("SubPageTransitionIn");
                fadeInAnimation.Begin(CustomersFrame);
            };
            fadeOutAnimation.Begin(CustomersFrame);
        }
    }
}
