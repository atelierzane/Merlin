using System.Text;
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

using MerlinCommunicator.Helpers;
using MerlinCommunicator.Style;
using MerlinCommunicator.Style.Class;
using MerlinCommunicator.Controls;

namespace MerlinCommunicator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
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

            var titleBar = this.FindName("windowTitleBar") as WindowTitleBar;
            if (titleBar != null)
            {
                titleBar.Title = "Updated Title - Merlin ROS";
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

        }
    }
}