using MerlinPointOfSale.Controls;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MerlinPointOfSale
{
    /// <summary>
    /// Interaction logic for WindowBase.xaml
    /// </summary>
    public partial class WindowBase : Window
    {
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;
        public WindowBase()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply the blur effect
            var blurEffect = new WindowBlurEffect(this);
            blurEffect.EnableBlur();


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

            // Optional: Add a slight delay before starting content animations
            var contentGridAnimation = FindResource("WindowAnimation") as Storyboard;
            if (contentGridAnimation != null)
            {
                contentGridAnimation.Begin(this);
            }
        }




    }
}