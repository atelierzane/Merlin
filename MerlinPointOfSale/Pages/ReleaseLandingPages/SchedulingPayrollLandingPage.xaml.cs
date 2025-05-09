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
    /// Interaction logic for SchedulingPayrollLandingPage.xaml
    /// </summary>
    public partial class SchedulingPayrollLandingPage : Page
    {
        public SchedulingPayrollLandingPage()
        {
            InitializeComponent();

            // Set menu items for the MenuBarControl
            MenuBar.MenuItems = new ObservableCollection<string>
            {
                "Time Clock",
                "Schedule",
                "Planner",
                "Time Cards",
            };

            MenuBar.ButtonClicked += MenuBar_ButtonClicked;


        }

        private void MenuBar_ButtonClicked(object sender, string buttonName)
        {
            switch (buttonName)
            {
                case "Time Clock":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleaseSchedulingPayrollPages.TimePunchPage());
                    break;

                case "Schedule":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleaseSchedulingPayrollPages.SchedulePage());
                    break;

                case "Planner":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleaseSchedulingPayrollPages.PlannerPage());
                    break;

                case "Time Cards":
                    NavigateToPage(new MerlinPointOfSale.Pages.ReleaseSchedulingPayrollPages.TimeCardsPage());

                    break;

                case "Approvals":

                    break;


            }
        }

        private void NavigateToPage(Page page)
        {
            var fadeOutAnimation = (Storyboard)FindResource("SubPageTransitionOut");
            fadeOutAnimation.Completed += (s, _) =>
            {
                SchedulingPayrollFrame.Content = page;
                var fadeInAnimation = (Storyboard)FindResource("SubPageTransitionIn");
                fadeInAnimation.Begin(SchedulingPayrollFrame);
            };
            fadeOutAnimation.Begin(SchedulingPayrollFrame);
        }
    }
}
