using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MerlinPointOfSale.Pages.ReleasePerformancePages;

namespace MerlinPointOfSale.Pages.ReleaseLandingPages
{
    public partial class PerformanceLandingPage : Page
    {
        public PerformanceLandingPage()
        {
            InitializeComponent();
            MenuBar.MenuItems = new ObservableCollection<string> { "Dashboard", "Sales", "KPI Performance" };
            MenuBar.ButtonClicked += MenuBar_ButtonClicked; // Subscribe to the control event
        }

        private void MenuBar_ButtonClicked(object sender, string buttonName)
        {
            switch (buttonName)
            {
                case "Dashboard":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleasePerformancePages.PerformanceDashboardPage());
                    break;
                case "Sales":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleasePerformancePages.PerformanceSalesPage());
                    break;
                case "KPI Performance":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleasePerformancePages.PerformanceKPIPage());
                    break;
            }
        }


        private void NavigateToPage(Page page)
        {
            var fadeOutAnimation = (Storyboard)FindResource("SubPageTransitionOut");
            fadeOutAnimation.Completed += (s, _) =>
            {
                PerformanceFrame.Content = page;
                var fadeInAnimation = (Storyboard)FindResource("SubPageTransitionIn");
                fadeInAnimation.Begin(PerformanceFrame);
            };
            fadeOutAnimation.Begin(PerformanceFrame);
        }
    }
}
