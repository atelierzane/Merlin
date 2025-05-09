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
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
using System.Windows.Media.Animation;
using MerlinPointOfSale.Controls;

namespace MerlinPointOfSale.Windows.DialogWindows
{
    /// <summary>
    /// Interaction logic for ResumeTransactionWindow1.xaml
    /// </summary>
    public partial class ResumeTransactionWindow1 : Window
    {
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;
        public Transaction SelectedTransaction { get; private set; }

        public ResumeTransactionWindow1(List<Transaction> suspendedTransactions)
        {
            InitializeComponent(); // Ensure all controls like DataGrid are initialized first

            // Safe filtering
            List<Transaction> todaysTransactions = new List<Transaction>();

            foreach (var t in suspendedTransactions)
            {
                try
                {
                    if (t.TransactionDate.Date == DateTime.Today) // if DateTime, not nullable
                    {
                        todaysTransactions.Add(t);
                    }
                }
                catch
                {
                    // Optional: log or debug if needed
                    // MessageBox.Show($"Invalid date on transaction: {t.TransactionId}");
                }
            }

            SuspendedTransactionsDataGrid.ItemsSource = todaysTransactions;

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

            var titleBar = this.FindName("windowTitleBar") as DialogWindowTitleBar;
            if (titleBar != null)
            {
                titleBar.Title = "Transaction Search";
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


        private void OnResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuspendedTransactionsDataGrid.SelectedItem is Transaction transaction)
            {
                SelectedTransaction = transaction;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a transaction to resume.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}
