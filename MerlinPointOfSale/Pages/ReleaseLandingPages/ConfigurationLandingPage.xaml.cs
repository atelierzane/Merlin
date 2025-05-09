using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MerlinPointOfSale.Pages.ReleaseLandingPages
{
    public partial class ConfigurationLandingPage : Page
    {
        public ConfigurationLandingPage()
        {
            InitializeComponent();

            // Set menu items for the MenuBarControl
            MenuBar.MenuItems = new ObservableCollection<string>
            {
                "Location and Register",
                "Taxes / Financial",
                "Quick Select Settings"
            };

            MenuBar.ButtonClicked += MenuBar_ButtonClicked;

            // Set the default page
            NavigateToPage(new MerlinPointOfSale.Pages.ReleaseConfigurationPages.LocationSettingsPage());
        }

        private void MenuBar_ButtonClicked(object sender, string buttonName)
        {
            switch (buttonName)
            {
                case "Location and Register":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleaseConfigurationPages.LocationSettingsPage());
                    break;

                case "Taxes / Financial":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleaseConfigurationPages.FinancialSettingsPage());
                    break;

                case "Quick Select Settings":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleaseConfigurationPages.QuickSelectSettingsPage());
                    break;
            }
        }

        private void NavigateToPage(Page page)
        {
            var fadeOutAnimation = (Storyboard)FindResource("SubPageTransitionOut");
            fadeOutAnimation.Completed += (s, _) =>
            {
                ConfigurationFrame.Content = page;
                var fadeInAnimation = (Storyboard)FindResource("SubPageTransitionIn");
                fadeInAnimation.Begin(ConfigurationFrame);
            };
            fadeOutAnimation.Begin(ConfigurationFrame);
        }
    }
}
