using MerlinPointOfSale.Models;
using MerlinPointOfSale.Windows.DialogWindows.DialogWindowsPages.ViewEditTransactionPages;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Windows.DialogWindows;
using MerlinPointOfSale.Style.Class;
using MerlinPointOfSale.Controls;
using MerlinPointOfSale.Pages.ReleaseLandingPages;
using System.Collections.ObjectModel;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    public partial class ViewEditTransactionDialogWindow : Window
    {

        public InputHelper inputHelper;
        public VisualEffectsHelper visualEffectsHelper;
        public List<SummaryItem> TransactionItems { get; set; }

        public ViewEditTransactionDialogWindow(List<SummaryItem> transactionItems)
        {
            InitializeComponent();
            // Set menu items for the MenuBarControl
            MenuBar.MenuItems = new ObservableCollection<string>
            {
                "Manual Discount",
                "Shopworn Discount",
                "Tax Exemption",
            };

            MenuBar.ButtonClicked += MenuBar_ButtonClicked;
            TransactionItems = transactionItems;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize VisualEffectsHelper
            visualEffectsHelper = new VisualEffectsHelper(this, mainBorder, glowEffectCanvas, glowSeparator, glowSeparatorBG);
            inputHelper = new InputHelper(this, visualEffectsHelper);
            // Apply the Acrylic Blur Effect
            var blurEffect = new WindowBlurEffect(this) { BlurOpacity = 0.85 };

            // Trigger the border glow effect on window load
            visualEffectsHelper.AdjustBorderGlow(new Point(mainBorder.ActualWidth / 2, mainBorder.ActualHeight / 2));

            var titleBar = this.FindName("windowTitleBar") as DialogWindowTitleBar;
            if (titleBar != null)
            {
                titleBar.Title = "Edit Transaction";
            }

            // Animate the window's top position
            DoubleAnimation topAnimation = new DoubleAnimation
            {
                From = this.Top - 15,
                To = this.Top,
                Duration = TimeSpan.FromSeconds(0.55),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };

            // Optional: Add a slight delay before starting content animations
            var contentGridAnimation = FindResource("DialogWindowAnimation_NoFlash") as Storyboard;
            if (contentGridAnimation != null)
            {
                contentGridAnimation.Begin(this);
            }

            this.BeginAnimation(Window.TopProperty, topAnimation);
        }

        private void MenuBar_ButtonClicked(object sender, string buttonName)
        {
            switch (buttonName)
            {
                case "Manual Discount":
                    NavigateToPage(new MerlinPointOfSale.Windows.DialogWindows.DialogWindowsPages.ViewEditTransactionPages.ManualDiscountPage(TransactionItems));
                    break;

                case "Shopworn Discount":

                    break;

                case "Tax Exemption":

                    break;

            }
        }

        private void NavigateToPage(Page page)
        {
            var fadeOutAnimation = (Storyboard)FindResource("PageTransitionOut");
            fadeOutAnimation.Completed += (s, _) =>
            {
                viewEditTransactionFrame.Content = page;
                var fadeInAnimation = (Storyboard)FindResource("PageTransitionIn");
                fadeInAnimation.Begin(viewEditTransactionFrame);
            };
            fadeOutAnimation.Begin(viewEditTransactionFrame);
        }

        private void BtnOnManualDiscount_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new ManualDiscountPage(TransactionItems));
        }

        // Handle when the window is closed, so discounts are applied
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Notify the PointOfSaleWindow that the TransactionItems have been updated
            if (Owner is PointOfSaleWindow posWindow)
            {
                posWindow.RefreshSummaryItems(TransactionItems);
            }
        }

        private void BtnOnShopwornDiscount_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void BtnOnPriceOverride_Click(object sender, RoutedEventArgs e)
        {
            //
        }

        private void BtnOnTaxExemption_Click(object sender, RoutedEventArgs e)
        {
            //
        }
    }
}
