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
using MerlinPointOfSale.Pages.ReleaseHomePages;

namespace MerlinPointOfSale.Pages.ReleaseLandingPages
{
    /// <summary>
    /// Interaction logic for HomeLandingPage.xaml
    /// </summary>
    public partial class HomeLandingPage : Page
    {
        public HomeLandingPage()
        {
            InitializeComponent();


            NavigateToPage(new HomePage());
        }

        private void NavigateToPage(Page page)
        {
            var fadeOutAnimation = (Storyboard)FindResource("SubPageTransitionOut");
            fadeOutAnimation.Completed += (s, _) =>
            {
                HomeFrame.Content = page;
                var fadeInAnimation = (Storyboard)FindResource("SubPageTransitionIn");
                fadeInAnimation.Begin(HomeFrame);
            };
            fadeOutAnimation.Begin(HomeFrame);
        }
    }
}
