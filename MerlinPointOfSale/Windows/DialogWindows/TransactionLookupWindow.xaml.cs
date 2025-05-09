using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Interaction logic for TransactionLookupWindow.xaml
    /// </summary>
    public partial class TransactionLookupWindow : Window
    {
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();
        private VisualEffectsHelper visualEffectsHelper;
        private InputHelper inputHelper;

        public TransactionLookupWindow()
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

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string locationID = Properties.Settings.Default.LocationID;
            DateTime? transactionDate = TransactionDatePicker.SelectedDate;
            string transactionID = TransactionIDTextBox.Text.Trim();
            string customerID = CustomerIDTextBox.Text.Trim();

            List<Transaction> transactions = SearchTransactions(locationID, transactionDate, transactionID, customerID);
            TransactionsDataGrid.ItemsSource = transactions;
        }

        private List<Transaction> SearchTransactions(string locationID, DateTime? transactionDate, string transactionID, string customerID)
        {
            List<Transaction> results = new List<Transaction>();
            string query = @"
    SELECT 
        t.TransactionID, 
        t.TransactionNumber, 
        t.RegisterNumber, 
        t.EmployeeID,
        t.TransactionDate, 
        t.TransactionTime, 
        t.CustomerID, 
        t.Subtotal, 
        t.Taxes, 
        t.TotalAmount, 
        t.NetCash, 
        t.PaymentMethod, 
        t.IsSuspended, 
        t.IsPostVoid
    FROM 
        Transactions t
    WHERE 
        t.LocationID = @LocationID
        AND (@TransactionDate IS NULL OR t.TransactionDate = @TransactionDate)
        AND (@TransactionID IS NULL OR t.TransactionID LIKE '%' + @TransactionID + '%')
        AND (@CustomerID IS NULL OR t.CustomerID LIKE '%' + @CustomerID + '%')
    ORDER BY 
        t.TransactionDate DESC, 
        t.TransactionTime DESC;";

            using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@LocationID", locationID);
                command.Parameters.AddWithValue("@TransactionDate", (object)transactionDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@TransactionID", string.IsNullOrEmpty(transactionID) ? DBNull.Value : transactionID);
                command.Parameters.AddWithValue("@CustomerID", string.IsNullOrEmpty(customerID) ? DBNull.Value : customerID);

                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new Transaction
                        {
                            TransactionId = reader["TransactionID"]?.ToString() ?? string.Empty,
                            TransactionNumber = reader["TransactionNumber"] as int? ?? 0,
                            RegisterNumber = reader["RegisterNumber"]?.ToString() ?? string.Empty,
                            EmployeeID = reader["EmployeeID"]?.ToString() ?? string.Empty,
                            TransactionDate = reader["TransactionDate"] as DateTime? ?? DateTime.MinValue,
                            TransactionTime = ((DateTime)reader["TransactionDate"]).Add((TimeSpan)reader["TransactionTime"]),
                            CustomerID = reader["CustomerID"]?.ToString() ?? string.Empty,
                            Subtotal = reader["Subtotal"] as decimal? ?? 0m,
                            Taxes = reader["Taxes"] as decimal? ?? 0m,
                            TotalAmount = reader["TotalAmount"] as decimal? ?? 0m,
                            NetCash = reader["NetCash"] as decimal? ?? 0m,
                            PaymentMethod = reader["PaymentMethod"]?.ToString() ?? string.Empty,
                            IsSuspended = reader["IsSuspended"] as bool? ?? false,
                            IsPostVoid = reader["IsPostVoid"] as bool? ?? false,
                        });
                    }
                }
            }

            return results;
        }

        private void CopyTransactionId_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionsDataGrid.SelectedItem is Transaction selectedTransaction)
            {
                Clipboard.SetText(selectedTransaction.TransactionId);
            }
            else
            {
                MessageBox.Show("Please select a transaction first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


    }

}
