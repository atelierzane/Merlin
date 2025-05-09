using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Windows.DialogWindows;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MerlinPointOfSale.Properties;
using System.Drawing.Printing;
using System.Drawing;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Windows.AlertWindows;
using System.Windows.Media.Animation;
using System.Diagnostics;
using MerlinPointOfSale.Windows;

namespace MerlinPointOfSale.Pages.ReleasePointOfSalePages
{
    /// <summary>
    /// Interaction logic for PointOfSaleLandingPage.xaml
    /// </summary>
    public partial class RegisterFunctionsPage : Page
    {
        MerlinPointOfSale.Helpers.DatabaseHelper databaseHelper = new MerlinPointOfSale.Helpers.DatabaseHelper();
        MerlinPointOfSale.Helpers.ReceiptHelper receiptHelper = new MerlinPointOfSale.Helpers.ReceiptHelper();

        private ApplicationHelper applicationHelper = new ApplicationHelper();
        public RegisterFunctionsPage()
        {
            InitializeComponent();
        }

        private void OnStartTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            TransactionLoginWindow verificationWindow = new TransactionLoginWindow();
            bool? dialogResult = verificationWindow.ShowDialog();

            if (dialogResult == true)
            {
                // Assuming EmployeeNumber and EmployeeFirstName are already fetched in the TransactionLoginWindow
                string employeeID = verificationWindow.EmployeeID;
                string employeeFirstName = verificationWindow.EmployeeFirstName;

                // Check if employee details are successfully retrieved
                if (!string.IsNullOrEmpty(employeeID) && !string.IsNullOrEmpty(employeeFirstName))
                {
                    // Minimize and hide the shadow of the Main Window
                    var mainWindow = Application.Current.MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.Visibility = Visibility.Collapsed;
                    }

                    // Open the Point of Sale window
                    PointOfSaleWindow_Release pointOfSaleWindow = new PointOfSaleWindow_Release(employeeID, employeeFirstName);

                    // Show the Point of Sale window as a modal dialog
                    pointOfSaleWindow.ShowDialog();

                    // Once the Point of Sale window is closed, restore the Main Window
                    if (mainWindow != null)
                    {
                        mainWindow.Visibility = Visibility.Visible;
                        mainWindow.WindowState = WindowState.Normal; // Restore window to normal state
                    }
                }
                else
                {
                    MessageBox.Show("Employee details not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void CashDropButton_Click(object sender, RoutedEventArgs e)
        {
            var verificationWindow = new TransactionLoginWindow();
            if (verificationWindow.ShowDialog() == true)
            {
                string employeeNumber = verificationWindow.EmployeeID;
                if (!string.IsNullOrEmpty(employeeNumber))
                {
                    var cashDropAmountWindow = new CashDropWindow();
                    if (cashDropAmountWindow.ShowDialog() == true)
                    {
                        decimal cashDropAmount = cashDropAmountWindow.CashDropAmount;
                        SaveCashDropTransaction(employeeNumber, cashDropAmount);
                        PrintCashDropSlipAndOpenDrawer();
                        CashdropSuccessWindow cashdropSuccessWindow = new CashdropSuccessWindow();
                        cashdropSuccessWindow.ShowDialog();
                    }
                    else
                    {
                        //MessageBox.Show("Cash Drop cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
//
                }
            }
            else
            {
                //MessageBox.Show("Cash Drop cancelled.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DialogWindowTest_Click(object sender, RoutedEventArgs e)
        {
            var alertWindowBase = new DialogWindowBase();
            alertWindowBase.ShowDialog();
        }

        private void OnCloseForPolling_Click(object sender, RoutedEventArgs e)
        {
            string merlinPollingAgentPath = applicationHelper.GetMerlinPollingAgentPath();
            Process.Start(merlinPollingAgentPath, "/bypassOpenCheck /closeStore");
        }



        private void NoSaleButton_Click(object sender, RoutedEventArgs e)
        {
            OpenVerificationWindowAndSaveTransaction("No Sale");
        }





        private void OpenVerificationWindowAndSaveTransaction(string transactionType)
        {
            var verificationWindow = new TransactionLoginWindow();
            if (verificationWindow.ShowDialog() == true)
            {
                string employeeID = verificationWindow.EmployeeID;
                if (!string.IsNullOrEmpty(employeeID))
                {
                    SaveSpecialTransaction(transactionType, employeeID);
                    if (transactionType == "No Sale")
                    {
                        PrintNoSaleSlipAndOpenDrawer();
                    }

                    else if (transactionType == "Cash Drop")
                    {
                        PrintCashDropSlipAndOpenDrawer();
                    }
                    //
                }
                else
                {
                    //
                }

            }
        }

        private void SaveSpecialTransaction(string paymentMethod, string employeeID)
        {
            string connectionString = databaseHelper.GetConnectionString();
            int transactionNumber = GetNextTransactionNumber();
            string registerNumber = Settings.Default.RegisterNumber;
            string locationID = Settings.Default.LocationID;
            DateTime transactionDate = DateTime.Now;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlTransaction dbTransaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = dbTransaction;

                            // Insert into Transactions table
                            command.CommandText = @"
                        INSERT INTO Transactions (
                            TransactionID, TransactionNumber, RegisterNumber, LocationID, TransactionDate, 
                            TransactionTime, EmployeeID, PaymentMethod, TotalAmount, Subtotal, Taxes, Fees, 
                            DiscountsLoyalty, DiscountsManual, NetCash, IsSuspended, IsPostVoid
                        )
                        VALUES (
                            @TransactionID, @TransactionNumber, @RegisterNumber, @LocationID, @TransactionDate, 
                            @TransactionTime, @EmployeeID, @PaymentMethod, @TotalAmount, @Subtotal, @Taxes, @Fees, 
                            @DiscountsLoyalty, @DiscountsManual, @NetCash, @IsSuspended, @IsPostVoid
                        )";

                            command.Parameters.Add(new SqlParameter("@TransactionID", SqlDbType.Char) { Value = Guid.NewGuid().ToString() });
                            command.Parameters.Add(new SqlParameter("@TransactionNumber", SqlDbType.Int) { Value = transactionNumber });
                            command.Parameters.Add(new SqlParameter("@RegisterNumber", SqlDbType.Char) { Value = registerNumber });
                            command.Parameters.Add(new SqlParameter("@LocationID", SqlDbType.Char) { Value = locationID });
                            command.Parameters.Add(new SqlParameter("@TransactionDate", SqlDbType.Date) { Value = transactionDate.Date });
                            command.Parameters.Add(new SqlParameter("@TransactionTime", SqlDbType.Time) { Value = transactionDate.TimeOfDay });
                            command.Parameters.Add(new SqlParameter("@EmployeeID", SqlDbType.Char) { Value = employeeID });
                            command.Parameters.Add(new SqlParameter("@PaymentMethod", SqlDbType.Char) { Value = paymentMethod });
                            command.Parameters.Add(new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = 0 }); // TotalAmount is 0 for special transactions
                            command.Parameters.Add(new SqlParameter("@Subtotal", SqlDbType.Decimal) { Value = 0 }); // No items, so Subtotal is 0
                            command.Parameters.Add(new SqlParameter("@Taxes", SqlDbType.Decimal) { Value = 0 });
                            command.Parameters.Add(new SqlParameter("@Fees", SqlDbType.Decimal) { Value = 0 });
                            command.Parameters.Add(new SqlParameter("@DiscountsLoyalty", SqlDbType.Decimal) { Value = 0 });
                            command.Parameters.Add(new SqlParameter("@DiscountsManual", SqlDbType.Decimal) { Value = 0 });
                            command.Parameters.Add(new SqlParameter("@NetCash", SqlDbType.Decimal) { Value = 0 }); // NetCash is 0 for special transactions
                            command.Parameters.Add(new SqlParameter("@IsSuspended", SqlDbType.Bit) { Value = false });
                            command.Parameters.Add(new SqlParameter("@IsPostVoid", SqlDbType.Bit) { Value = false });

                            command.ExecuteNonQuery();

                            dbTransaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in SaveSpecialTransaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCashDropTransaction(string employeeNumber, decimal cashDropAmount)
        {
            string connectionString = databaseHelper.GetConnectionString();
            int transactionNumber = GetNextTransactionNumber();
            string registerNumber = Settings.Default.RegisterNumber;
            string locationID = Settings.Default.LocationID;
            DateTime transactionDate = DateTime.Now;
            string paymentMethod = $"Cash Drop: {cashDropAmount:C2}"; // Include the cash drop amount in the payment method

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlTransaction dbTransaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = dbTransaction;

                            // Insert into Transactions table
                            command.CommandText = @"
                    INSERT INTO Transactions (
                        TransactionID, TransactionNumber, RegisterNumber, LocationID, TransactionDate, 
                        TransactionTime, EmployeeID, PaymentMethod, TotalAmount, Subtotal, Taxes, Fees, 
                        DiscountsLoyalty, DiscountsManual, NetCash, IsSuspended, IsPostVoid
                    ) 
                    VALUES (
                        @TransactionID, @TransactionNumber, @RegisterNumber, @LocationID, @TransactionDate, 
                        @TransactionTime, @EmployeeID, @PaymentMethod, @TotalAmount, @Subtotal, @Taxes, @Fees, 
                        @DiscountsLoyalty, @DiscountsManual, @NetCash, @IsSuspended, @IsPostVoid
                    )";

                            // Parameters
                            command.Parameters.Add(new SqlParameter("@TransactionID", SqlDbType.Char) { Value = Guid.NewGuid().ToString() });
                            command.Parameters.Add(new SqlParameter("@TransactionNumber", SqlDbType.Int) { Value = transactionNumber });
                            command.Parameters.Add(new SqlParameter("@RegisterNumber", SqlDbType.Char) { Value = registerNumber });
                            command.Parameters.Add(new SqlParameter("@LocationID", SqlDbType.Char) { Value = locationID });
                            command.Parameters.Add(new SqlParameter("@TransactionDate", SqlDbType.Date) { Value = transactionDate.Date });
                            command.Parameters.Add(new SqlParameter("@TransactionTime", SqlDbType.Time) { Value = transactionDate.TimeOfDay });
                            command.Parameters.Add(new SqlParameter("@EmployeeID", SqlDbType.Char) { Value = employeeNumber });
                            command.Parameters.Add(new SqlParameter("@PaymentMethod", SqlDbType.Char) { Value = paymentMethod });
                            command.Parameters.Add(new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = cashDropAmount }); // Cash drop amount
                            command.Parameters.Add(new SqlParameter("@Subtotal", SqlDbType.Decimal) { Value = 0 }); // No items, so Subtotal is 0
                            command.Parameters.Add(new SqlParameter("@Taxes", SqlDbType.Decimal) { Value = 0 });
                            command.Parameters.Add(new SqlParameter("@Fees", SqlDbType.Decimal) { Value = 0 });
                            command.Parameters.Add(new SqlParameter("@DiscountsLoyalty", SqlDbType.Decimal) { Value = 0 });
                            command.Parameters.Add(new SqlParameter("@DiscountsManual", SqlDbType.Decimal) { Value = 0 });
                            command.Parameters.Add(new SqlParameter("@NetCash", SqlDbType.Decimal) { Value = cashDropAmount }); // Net cash equals cash drop amount
                            command.Parameters.Add(new SqlParameter("@IsSuspended", SqlDbType.Bit) { Value = false });
                            command.Parameters.Add(new SqlParameter("@IsPostVoid", SqlDbType.Bit) { Value = false });

                            command.ExecuteNonQuery();
                            dbTransaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in SaveCashDropTransaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public int GetNextTransactionNumber()
        {
            int nextTransactionNumber = 1; // Start with 1 for a new day
            string maxQuery = "SELECT MAX(TransactionNumber) FROM Transactions WHERE TransactionDate = @TransactionDate";
            string existsQuery = "SELECT COUNT(*) FROM Transactions WHERE TransactionDate = @TransactionDate AND TransactionNumber = @TransactionNumber";

            try
            {
                // Step 1: Fetch the current max transaction number for the day
                SqlParameter[] maxParams = {
            new SqlParameter("@TransactionDate", SqlDbType.Date) { Value = DateTime.Now.Date }
        };

                object maxResult = databaseHelper.ExecuteScalarQuery(maxQuery, maxParams);

                if (maxResult != DBNull.Value && maxResult != null && int.TryParse(maxResult.ToString(), out int currentMaxTransactionNumber))
                {
                    nextTransactionNumber = currentMaxTransactionNumber + 1;
                }

                // Step 2: Check if the transaction number already exists
                bool exists = true;

                while (exists)
                {
                    SqlParameter[] existsParams = {
                new SqlParameter("@TransactionDate", SqlDbType.Date) { Value = DateTime.Now.Date },
                new SqlParameter("@TransactionNumber", SqlDbType.Int) { Value = nextTransactionNumber }
            };

                    object existsResult = databaseHelper.ExecuteScalarQuery(existsQuery, existsParams);
                    exists = existsResult != null && Convert.ToInt32(existsResult) > 0;

                    if (exists)
                    {
                        nextTransactionNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in GetNextTransactionNumber: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return nextTransactionNumber;
        }

        private void PrintNoSaleSlipAndOpenDrawer()
        {
            string receiptPrinterName = receiptHelper.GetPrinterName_Epson();
            using (PrintDocument printDocument = new PrintDocument())
            {
                printDocument.PrinterSettings.PrinterName = receiptPrinterName;
                printDocument.PrintPage += new PrintPageEventHandler(receiptHelper.PrintNoSaleDocument_PrintPage);

                try
                {
                    printDocument.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error printing no-sale slip: {ex.Message}", "Printing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } // Using statement ensures Dispose is called, even if an exception occurs

            // Drawer command is sent after ensuring printing does not cause an exception
            try
            {
                byte[] drawerOpenCommand = new byte[] { 27, 112, 0, 25, 250 };
                RawPrinterHelper.SendBytesToPrinter(receiptPrinterName, drawerOpenCommand);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending drawer open command: {ex.Message}", "Drawer Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintCashDropSlipAndOpenDrawer()
        {
            string receiptPrinterName = receiptHelper.GetPrinterName_Epson();
            using (PrintDocument printDocument = new PrintDocument())
            {
                printDocument.PrinterSettings.PrinterName = receiptPrinterName;
                printDocument.PrintPage += new PrintPageEventHandler(receiptHelper.PrintCashDropDocument_PrintPage);

                try
                {
                    printDocument.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error printing cash drop slip: {ex.Message}", "Printing Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } // Using statement ensures Dispose is called, even if an exception occurs

            // Drawer command is sent after ensuring printing does not cause an exception
            try
            {
                byte[] drawerOpenCommand = new byte[] { 27, 112, 0, 25, 250 };
                RawPrinterHelper.SendBytesToPrinter(receiptPrinterName, drawerOpenCommand);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending drawer open command: {ex.Message}", "Drawer Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnResumeTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Fetch suspended transactions
                List<Transaction> suspendedTransactions = GetSuspendedTransactions();

                if (suspendedTransactions.Any())
                {
                    ResumeTransactionWindow1 resumeWindow = new ResumeTransactionWindow1(suspendedTransactions);
                    bool? result = resumeWindow.ShowDialog();

                    if (result == true && resumeWindow.SelectedTransaction != null)
                    {
                        Transaction selectedTransaction = resumeWindow.SelectedTransaction;

                        // Open the selected transaction in PointOfSaleWindow
                        PointOfSaleWindow_Release pointOfSaleWindow = new PointOfSaleWindow_Release(selectedTransaction.EmployeeID, "Resuming Transaction");
                        pointOfSaleWindow.LoadSuspendedTransaction(selectedTransaction.TransactionId);
                        pointOfSaleWindow.ShowDialog();
                    }
                }
                else
                {
                    MessageBox.Show("No suspended transactions found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching suspended transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private List<Transaction> GetSuspendedTransactions()
        {
            List<Transaction> transactions = new List<Transaction>();

            string query = @"
        SELECT TransactionId, TransactionNumber, RegisterNumber, LocationID, TransactionDate, 
               TransactionTime, EmployeeID, CustomerID, Subtotal, Taxes, TotalAmount,
               PaymentMethod, NetCash, IsSuspended
        FROM Transactions
        WHERE IsSuspended = 1";

            using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new Transaction
                            {
                                TransactionId = Convert.ToString(reader["TransactionId"]),
                                TransactionNumber = Convert.ToInt32(reader["TransactionNumber"]),
                                RegisterNumber = reader["RegisterNumber"].ToString(),
                                LocationID = reader["LocationID"].ToString(),
                                TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                                TransactionTime = Convert.ToDateTime(reader["TransactionTime"].ToString()),
                                EmployeeID = reader["EmployeeID"].ToString(),
                                CustomerID = reader["CustomerID"].ToString(),
                                Subtotal = Convert.ToDecimal(reader["Subtotal"]),
                                Taxes = Convert.ToDecimal(reader["Taxes"]),
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                PaymentMethod = reader["PaymentMethod"].ToString(),
                                NetCash = Convert.ToDecimal(reader["NetCash"]),
                                IsSuspended = Convert.ToBoolean(reader["IsSuspended"])
                            });
                        }
                    }
                }
            }

            return transactions;
        }


        private void OnResumeSelectedTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (SuspendedTransactionsDataGrid.SelectedItem is Transaction selectedTransaction)
            {
                try
                {
                    // Open the transaction in PointOfSaleWindow
                    PointOfSaleWindow pointOfSaleWindow = new PointOfSaleWindow(selectedTransaction.EmployeeID, "Resuming Transaction");
                    pointOfSaleWindow.LoadSuspendedTransaction(selectedTransaction.TransactionId);
                    pointOfSaleWindow.ShowDialog();

                    // Hide the resume grid
                    ResumeTransactionGrid.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error resuming transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a transaction to resume.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void OnCancelResumeTransaction_Click(object sender, RoutedEventArgs e)
        {
            ResumeTransactionGrid.Visibility = Visibility.Collapsed;
        }

        private (bool IsVerified, string EmployeeID, string EmployeeFirstName) VerifyEmployee()
        {
            // Instantiate and show the TransactionLoginWindow
            TransactionLoginWindow loginWindow = new TransactionLoginWindow();
            bool? result = loginWindow.ShowDialog();

            // Check if the login was successful
            if (result == true && !string.IsNullOrWhiteSpace(loginWindow.EmployeeID) && !string.IsNullOrWhiteSpace(loginWindow.EmployeeFirstName))
            {
                return (true, loginWindow.EmployeeID, loginWindow.EmployeeFirstName);
            }

            // If verification fails, show an error
            MessageBox.Show("Verification failed. Please try again.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            return (false, null, null);
        }

        private void OnLoadingAnimationTest_Click(object sender, RoutedEventArgs e)
        {
            // Show the loading animation
            LoadingAnimation.Visibility = Visibility.Visible;

            // Find the storyboard in the LoadingAnimation control's resources
            var storyboard = (Storyboard)LoadingAnimation.FindResource("BounceAnimation");

            // Start the animation
            storyboard.Begin(LoadingAnimation, true);

            // Simulate a task (e.g., loading data)
            Task.Run(() =>
            {
                // Simulate a delay for 3 seconds
                Thread.Sleep(3000);

                // Hide the animation after the task is complete
                Dispatcher.Invoke(() =>
                {
                    LoadingAnimation.Visibility = Visibility.Collapsed;
                });
            });
        }




        private void ReprintReceipt_Click(object sender, RoutedEventArgs e)
        {
            string transactionId = PromptForTransactionId();
            if (string.IsNullOrEmpty(transactionId))
            {
               
                return;
            }

            Transaction transaction = GetTransactionById(transactionId);
            if (transaction != null)
            {
                var receiptHelper = new ReceiptHelper();
                receiptHelper.PrintStandardReceipt(transaction, transaction.TransactionItems, transaction.EmployeeID, transaction.TransactionNumber);
                MessageBox.Show("Receipt reprinted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Transaction not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string PromptForTransactionId()
        {
            var qrScannerWindow = new QRScannerWindow(); // Create a window for scanning QR codes
            if (qrScannerWindow.ShowDialog() == true)
            {
                return qrScannerWindow.ScannedTransactionId;
            }
            return null;
        }
        private Transaction GetTransactionById(string transactionId)
        {
            string query = @"
    SELECT t.TransactionID, t.TransactionNumber, t.RegisterNumber, t.LocationID, t.TransactionDate,
       t.TransactionTime, t.EmployeeID, t.CustomerID, t.Subtotal, t.Taxes, t.DiscountsLoyalty,
       t.DiscountsManual, t.TotalAmount, t.NetCash, t.PaymentMethod, t.IsSuspended, t.IsPostVoid, 
       d.SKU, d.Quantity, c.Price, -- Always use Catalog.Price
       d.Description
FROM Transactions t
LEFT JOIN TransactionDetails d ON t.TransactionID = d.TransactionID
LEFT JOIN Catalog c ON d.SKU = c.SKU -- Join with Catalog to retrieve the correct Price
WHERE t.TransactionID = @TransactionID";
            ;

            using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionID", transactionId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            Transaction transaction = null;
                            List<TransactionItems> items = new List<TransactionItems>();

                            while (reader.Read())
                            {
                                if (transaction == null)
                                {
                                    transaction = new Transaction
                                    {
                                        TransactionId = reader["TransactionID"] as string ?? string.Empty,
                                        TransactionNumber = reader["TransactionNumber"] as int? ?? 0,
                                        RegisterNumber = reader["RegisterNumber"] as string ?? string.Empty,
                                        LocationID = reader["LocationID"] as string ?? string.Empty,
                                        TransactionDate = reader["TransactionDate"] as DateTime? ?? DateTime.MinValue,
                                        TransactionTime = reader["TransactionTime"] != DBNull.Value
                                            ? ((DateTime)reader["TransactionDate"]).Add((TimeSpan)reader["TransactionTime"])
                                            : DateTime.MinValue, // Fallback to MinValue if either value is null
                                        EmployeeID = reader["EmployeeID"] as string ?? string.Empty,
                                        CustomerID = reader["CustomerID"] as string, // Can be null
                                        Subtotal = reader["Subtotal"] as decimal? ?? 0m,
                                        Taxes = reader["Taxes"] as decimal? ?? 0m,
                                        Discounts = (reader["DiscountsLoyalty"] as decimal? ?? 0m) +
                                                    (reader["DiscountsManual"] as decimal? ?? 0m),
                                        TotalAmount = reader["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAmount"]) : 0m,
                                        NetCash = reader["NetCash"] as decimal? ?? 0m,
                                        PaymentMethod = reader["PaymentMethod"] as string ?? string.Empty,
                                        IsSuspended = reader["IsSuspended"] as bool? ?? false,
                                        TransactionItems = new List<TransactionItems>()
                                    };

                                }

                                // Add transaction items
                                if (!reader.IsDBNull(reader.GetOrdinal("SKU")))
                                {
                                    items.Add(new TransactionItems
                                    {
                                        SKU = reader["SKU"] as string ?? string.Empty,
                                        Quantity = reader["Quantity"] as int? ?? 0,
                                        Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0m,
                                        Description = reader["Description"] as string ?? string.Empty
                                    });
                                }
                            }

                            if (transaction != null)
                            {
                                transaction.TransactionItems = items;
                            }

                            return transaction;
                        }
                    }
                }
            }

            return null;
        }

        private void PrintGiftReceipt_Click(object sender, RoutedEventArgs e)
        {
            string transactionId = PromptForTransactionId();
            if (string.IsNullOrEmpty(transactionId))
            {
                
                return;
            }

            // Fetch items from the TransactionDetails table
            List<TransactionItems> transactionItems = GetTransactionItems(transactionId);
            if (transactionItems == null || !transactionItems.Any())
            {
                MessageBox.Show("No items found for the transaction.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Open selection dialog for gift receipt items
            var dialog = new GiftReceiptItemSelectionDialogReprint(transactionItems);
            if (dialog.ShowDialog() == true && dialog.SelectedItems.Any())
            {
                // Print the gift receipt
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = receiptHelper.GetPrinterName_Epson();

                printDocument.PrintPage += (sender, args) =>
                {
                    receiptHelper.PrintGiftReceiptPage(args.Graphics, dialog.SelectedItems, transactionId, transactionItems.FirstOrDefault()?.TransactionNumber ?? 0);
                };

                try
                {
                    printDocument.Print();
                    MessageBox.Show("Gift receipt printed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to print gift receipt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No items selected for the gift receipt.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private List<TransactionItems> GetTransactionItems(string transactionId)
        {
            List<TransactionItems> items = new List<TransactionItems>();
            string query = @"
        SELECT d.SKU, d.Quantity, c.Price, c.ProductName, d.Description, d.CategoryID, t.TransactionNumber
        FROM TransactionDetails d
        INNER JOIN Transactions t ON d.TransactionID = t.TransactionID
        LEFT JOIN Catalog c ON d.SKU = c.SKU
        WHERE d.TransactionID = @TransactionID";

            using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionID", transactionId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new TransactionItems
                            {
                                SKU = reader["SKU"].ToString(),
                                Quantity = reader["Quantity"] != DBNull.Value ? Convert.ToInt32(reader["Quantity"]) : 0,
                                Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0m,
                                ProductName = reader["ProductName"].ToString(), // Map ProductName
                                Description = reader["Description"].ToString(),
                                CategoryID = reader["CategoryID"].ToString(),
                                TransactionNumber = reader["TransactionNumber"] != DBNull.Value
                                    ? Convert.ToInt32(reader["TransactionNumber"])
                                    : 0
                            };

                            items.Add(item);
                        }
                    }
                }
            }

            return items;
        }

        private void TransactionLookup_Click(object sender, RoutedEventArgs e)
        {
            TransactionLookupWindow transactionLookupWindow = new TransactionLookupWindow();
            transactionLookupWindow.ShowDialog();
        }
    }
}
