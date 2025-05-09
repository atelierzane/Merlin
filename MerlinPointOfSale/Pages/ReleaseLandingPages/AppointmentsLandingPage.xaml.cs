using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MerlinPointOfSale.Pages.ReleaseLandingPages
{
    public partial class AppointmentsLandingPage : Page
    {
        public AppointmentsLandingPage()
        {
            InitializeComponent();

            // Set menu items for the MenuBarControl
            MenuBar.MenuItems = new ObservableCollection<string>
            {
                "Calendar",
                "Clients",
            };

            MenuBar.ButtonClicked += MenuBar_ButtonClicked;


        }

        private void MenuBar_ButtonClicked(object sender, string buttonName)
        {
            switch (buttonName)
            {
                case "Calendar":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleaseAppointmentsPages.CalendarPage());
                    break;

                case "Clients":
                    
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



