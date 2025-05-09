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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MerlinPointOfSale.Controls;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;
using System.Windows.Media.Animation;


namespace MerlinPointOfSale.Windows.DialogWindows
{
    /// <summary>
    /// Interaction logic for QRScannerWindow.xaml
    /// </summary>
    public partial class QRScannerWindow : Window
    {
        private ApplicationHelper applicationHelper;
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;
        public string ScannedTransactionId { get; private set; }

        public QRScannerWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize VisualEffectsHelper
            visualEffectsHelper = new VisualEffectsHelper(this, mainBorder, glowEffectCanvas, glowSeparator, glowSeparatorBG);
            inputHelper = new InputHelper(this, visualEffectsHelper);
            applicationHelper = new ApplicationHelper();

            // Apply the Acrylic Blur Effect
            var blurEffect = new WindowBlurEffect(this) { BlurOpacity = 0.85 };

            // Trigger the border glow effect on window load
            visualEffectsHelper.AdjustBorderGlow(new Point(mainBorder.ActualWidth / 2, mainBorder.ActualHeight / 2));

            // Animate the window's top position
            DoubleAnimation topAnimation = new DoubleAnimation
            {
                From = this.Top - 15,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.55),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };


            var titleBar = this.FindName("windowTitleBar") as WindowTitleBar;
            if (titleBar != null)
            {
                titleBar.Title = "Updated Title - Merlin ROS";
            }

            // Optional: Add a slight delay before starting content animations
            var contentGridAnimation = FindResource("DialogWindowAnimation_NoFlash") as Storyboard;
            if (contentGridAnimation != null)
            {
                contentGridAnimation.Begin(this);
            }
            this.BeginAnimation(Window.TopProperty, topAnimation);
        }
        private void OnConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            // Validate if the input is not empty
            if (!string.IsNullOrWhiteSpace(ScannedTextBox.Text))
            {
                ScannedTransactionId = ScannedTextBox.Text.Trim();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter or scan a valid Transaction ID.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
