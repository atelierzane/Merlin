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

namespace MerlinPointOfSale.Windows.DialogWindows
{
    /// <summary>
    /// Interaction logic for TransactionIDInputDialog.xaml
    /// </summary>
    public partial class TransactionIDInputDialog : Window
    {
        private ApplicationHelper applicationHelper = new ApplicationHelper();
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;
        public TransactionIDInputDialog()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            var titleBar = this.FindName("windowTitleBar") as WindowTitleBar;
            if (titleBar != null)
            {
                titleBar.Title = "Transaction Lookup";
            }
            // Play the custom notification sound
            applicationHelper.PlayCustomNotificationSound("MerlinPointOfSale.Resources.MerlinNotification_Error.wav");

            // Apply the blur effect
            var blurEffect = new WindowBlurEffect(this);
            blurEffect.EnableBlur();

            // Initialize VisualEffectsHelper
            visualEffectsHelper = new VisualEffectsHelper(this, mainBorder, glowEffectCanvas, glowSeparator, glowSeparatorBG);
            inputHelper = new InputHelper(this, visualEffectsHelper);

            // Start the alert animation
            var alertWindowAnimation = FindResource("AlertWindowAnimation") as Storyboard;
            if (alertWindowAnimation != null)
            {
                alertWindowAnimation.Begin(this);
            }
        }

        public string ResponseText
        {
            get { return InputTextBox.Text; }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
