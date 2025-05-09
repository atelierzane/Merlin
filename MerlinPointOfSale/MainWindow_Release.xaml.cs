using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Data.OleDb;

using MerlinPointOfSale.Controls;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
using MerlinPointOfSale.Pages;
using MerlinPointOfSale.Pages.ReleaseLandingPages;
using MerlinPointOfSale.Windows;
using MerlinPointOfSale.Windows.DialogWindows;

namespace MerlinPointOfSale
{
    /// <summary>
    /// Interaction logic for MainWindow_Release.xaml
    /// </summary>
    public partial class MainWindow_Release : Window
    {
        private ApplicationHelper applicationHelper = new ApplicationHelper();
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;

        public static readonly RoutedCommand ToggleSearchCommand = new RoutedCommand();
        public MainWindow_Release()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            CommandBindings.Add(new CommandBinding(ToggleSearchCommand, ExecuteToggleSearch));
            DelayNavigation();
        }

        private async void DelayNavigation()
        {
            await Task.Delay(1000); // Delay for 1 second
            NavigateToPage(new PointOfSaleLandingPage());
            // Trigger the alternate logo animation
            var showAlternateLogo = (Storyboard)FindResource("ShowAlternateLogo");
            showAlternateLogo.Begin(this);
        }

        // Command execution for toggling search
        private void ExecuteToggleSearch(object sender, ExecutedRoutedEventArgs e)
        {
            SearchToggleButton_Click(SearchToggleButton, null);
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply the Acrylic Blur Effect
            var blurEffect = new WindowBlurEffect(this) { BlurOpacity = 0.85 };

            // Initialize VisualEffectsHelper
            visualEffectsHelper = new VisualEffectsHelper(this, mainBorder, glowEffectCanvas, glowSeparator, glowSeparatorBG);
            inputHelper = new InputHelper(this, visualEffectsHelper);

            // Trigger the border glow effect on window load
            visualEffectsHelper.AdjustBorderGlow(new Point(mainBorder.ActualWidth / 2, mainBorder.ActualHeight / 2));

            var titleBar = this.FindName("windowTitleBar") as DialogWindowTitleBar;
            if (titleBar != null)
            {
                titleBar.Title = "Merlin Point of Sale";
            }
            // Animate the window's top position
            DoubleAnimation topAnimation = new DoubleAnimation
            {
                From = this.Top + 50,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };
            this.BeginAnimation(Window.TopProperty, topAnimation);


        }

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            bool isExpanded = MenuColumn.Width.Value > 60;

            // Animate the menu column size
            GridLengthAnimation animation = new GridLengthAnimation
            {
                From = MenuColumn.Width,
                To = new GridLength(isExpanded ? 60 : 215, GridUnitType.Pixel),
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };

            Storyboard.SetTarget(animation, MenuColumn);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Width"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin(this);

            // Trigger logo flip animations
            if (isExpanded)
            {
                var showAlternateLogo = (Storyboard)FindResource("ShowAlternateLogo");
                showAlternateLogo.Begin(this);
            }
            else
            {
                var showFullLogo = (Storyboard)FindResource("ShowFullLogo");
                showFullLogo.Begin(this);
            }
        }



        private void OnSearchButton_Checked(object sender, RoutedEventArgs e)
        {
            //
        }


        private void OnPerformanceButton_Checked(object sender, RoutedEventArgs e) => NavigateToPage(new PerformanceLandingPage());

        private void OnPointOfSaleButton_Checked(object sender, RoutedEventArgs e) => NavigateToPage(new PointOfSaleLandingPage());

        private void OnTasksButton_Checked(object sender, RoutedEventArgs e) => NavigateToPage(new HomePage());


        private async void OnConfigurationButton_Checked(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000); // Delay for 1 second
            ConfigurationWindow configurationWindow = new ConfigurationWindow();
            configurationWindow.ShowDialog();
        }
        private async void OnPollingAgentButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new PollingAgentPage());
            await Task.Delay(1000); // Delay for 1 second
            string merlinPollingAgentPath = applicationHelper.GetMerlinPollingAgentPath();
            Process.Start(merlinPollingAgentPath, "/bypassOpenCheck /closeStore");
        }

        private void OnCustomersButton_Checked(object sender, RoutedEventArgs e) => NavigateToPage(new CustomersLandingPage());


        private async void OnBackOfficeButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new BackOfficePage());
            await Task.Delay(1000); // Delay for 1 second

            try
            {
                string merlinBackOfficePath = applicationHelper.GetMerlinBackOfficePath();
                Process.Start(merlinBackOfficePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching Merlin Polling Agent: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCalendarButton_Checked(object sender, RoutedEventArgs e) => NavigateToPage(new AppointmentsLandingPage());

        private void OnSchedulingButton_Checked(object sender, RoutedEventArgs e) => NavigateToPage(new SchedulingPayrollLandingPage());


        private void NavigateToPage(Page page)
        {
            var fadeOutAnimation = (Storyboard)FindResource("PageTransitionOut");
            fadeOutAnimation.Completed += (s, _) =>
            {
                MainFrame.Content = page;
                var fadeInAnimation = (Storyboard)FindResource("PageTransitionIn");
                fadeInAnimation.Begin(MainFrame);
            };
            fadeOutAnimation.Begin(MainFrame);
        }

        private void OnPointOfSaleMenuButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private bool areButtonsSwitched = true;

        private void SearchToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // --- Show/Hide Search Bar ---
            bool isSearchRowVisible = SearchRow.Height != new GridLength(0);

            GridLengthAnimation rowAnimation = new GridLengthAnimation
            {
                From = SearchRow.Height,
                To = isSearchRowVisible ? new GridLength(0) : new GridLength(50),
                Duration = new Duration(TimeSpan.FromSeconds(0.2))
            };

            Storyboard.SetTarget(rowAnimation, SearchRow);
            Storyboard.SetTargetProperty(rowAnimation, new PropertyPath("Height"));

            Storyboard rowStoryboard = new Storyboard();
            rowStoryboard.Children.Add(rowAnimation);
            rowStoryboard.Begin(this);

            // --- Swap Buttons Vertically ---
            // Define target positions relative to their current positions
            double menuTargetY = areButtonsSwitched ? 40 : 0; // Move MenuToggleButton down or up
            double searchTargetY = areButtonsSwitched ? -40 : 0; // Move SearchToggleButton up or down

            // Create animations for vertical movement
            var menuAnimation = new DoubleAnimation
            {
                To = menuTargetY,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            var searchAnimation = new DoubleAnimation
            {
                To = searchTargetY,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            // Apply animations to adjust positions
            MenuButtonTransform.BeginAnimation(TranslateTransform.YProperty, menuAnimation);
            SearchButtonTransform.BeginAnimation(TranslateTransform.YProperty, searchAnimation);

            // Toggle the state
            areButtonsSwitched = !areButtonsSwitched;
        }


    }
}
