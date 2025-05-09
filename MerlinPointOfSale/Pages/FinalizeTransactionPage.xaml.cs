using System;
using System.Windows;
using System.Windows.Controls;
using MerlinPointOfSale.Models;
using MerlinPointOfSale.Repositories;
using System.Data.SqlClient;
using System.Collections.Generic;
using MerlinPointOfSale.Windows.DialogWindows;
using MerlinPointOfSale.Properties;
using System.Drawing.Printing;
using System.Drawing;
using System.Globalization;
using ZXing.Windows.Compatibility;
using ZXing;

namespace MerlinPointOfSale.Pages
{
    public partial class FinalizeTransactionPage : Page
    {

        private RawPrinterHelper rawPrinterHelper = new RawPrinterHelper();

        private decimal TotalDue;
        private decimal OriginalTotalDue;
        private decimal TotalPaidInCash = 0m;
        private string employeeID;
        private string transactionId;
        private TransactionSummary transactionSummary;
        private DatabaseHelper databaseHelper;
        private decimal subtotal;
        private decimal taxes;
        private decimal tipsTotal;
        private List<SummaryItem> summaryItems;
        private List<InventoryItem> saleItems; // List of sale items
        private string locationID;
        private string registerNumber;
        private int transactionNumber;


        private string cardBrand;
        private string cardLastFour;
        private int expMonth;
        private int expYear;
        private string chargeId;
        private string paymentMethod;

        private decimal discounts;

        private Customer customer;

        public delegate void TotalDueUpdatedHandler(decimal newTotalDue);
        public event TotalDueUpdatedHandler TotalDueUpdated;
        public event Action<string, string, int, int, string> CardPaymentCompleted;
        public Action<string, decimal, decimal> OnFinalizeTransaction;


        private ProductRepository productRepository; // Add this line

        public FinalizeTransactionPage(decimal totalDue, string employeeID, TransactionSummary transactionSummary, decimal subtotal, decimal taxes, decimal tipsTotal, List<SummaryItem> summaryItems, List<InventoryItem> saleItems, int transactionNumber, decimal discounts, Customer customer)
        {
            InitializeComponent();
            this.TotalDue = totalDue;
            this.employeeID = employeeID;
            this.transactionSummary = transactionSummary;
            this.subtotal = subtotal;
            this.taxes = taxes;
            this.summaryItems = summaryItems;
            this.saleItems = saleItems;
            this.transactionNumber = transactionNumber;
            this.discounts = discounts;
            this.tipsTotal = tipsTotal;




            string locationID = Settings.Default.LocationID;
            string registerNumber = Settings.Default.RegisterNumber;

            databaseHelper = new DatabaseHelper();
            productRepository = new ProductRepository(databaseHelper.GetConnectionString()); // Initialize productRepository here
            transactionId = GenerateTransactionId();
        }


        private void BtnOnCash_Click(object sender, RoutedEventArgs e)
        {
            CashPaymentWindow cashPaymentWindow = new CashPaymentWindow();
            if (cashPaymentWindow.ShowDialog() == true)
            {
                decimal cashReceived = cashPaymentWindow.CashReceived;
                TotalPaidInCash += cashReceived;

                if (TotalPaidInCash >= TotalDue)
                {
                    decimal change = TotalPaidInCash - TotalDue;
                    MessageBox.Show($"Change: ${change:F2}", "Transaction Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Finalize transaction
                    paymentMethod = "Cash";
                    SaveTransaction(paymentMethod, TotalDue, TotalPaidInCash);




                    TotalPaidInCash = 0m;
                    TotalDue = 0m;
                    transactionSummary.Total = 0m;
                }
                else
                {
                    TotalDue -= cashReceived;
                    MessageBox.Show($"Remaining Balance: ${TotalDue:F2}", "Partial Payment", MessageBoxButton.OK, MessageBoxImage.Information);
                    transactionSummary.Total = TotalDue;
                }
            }
        }

        private void BtnOnCard_Click(object sender, RoutedEventArgs e)
        {

            if (TotalDue < 0)
            {
                //When TotalDue is negative, process a refund
                decimal refundAmount = Math.Abs(TotalDue);
                string transactionId = PromptForTransactionId();

                if (!string.IsNullOrEmpty(transactionId))
                {
                    //If a Strip ChargeID is found, initiate the refund process
                    string chargeId = FetchChargeId(transactionId);

                    if (!string.IsNullOrEmpty(chargeId))
                    {
                        ProcessRefund(chargeId, refundAmount, transactionId);
                    }
                    else
                    {
                        MessageBox.Show("Invalid MerlinID or ChargeID not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("MerlinID is required for processing refunds.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

            ProcessPayment();
        }

        private void ProcessPayment()
        {
            CardPaymentWindow paymentWindow = new CardPaymentWindow(TotalDue);
            paymentWindow.PaymentCompleted += (sender, args) =>
            {
                if (args.status == "succeeded")
                {
                    // Store card payment details
                    StorePaymentDetails(args.cardBrand, args.cardLastFour, args.expMonth, args.expYear, args.chargeId);

                    // Save transaction, including card details
                    // Finalize transaction
                    paymentMethod = "Card";
                    SaveTransaction(paymentMethod, TotalDue, TotalPaidInCash); // ✅ Correct



                    MessageBox.Show("Payment Successful", "Transaction Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Reset state
                    TotalDue = 0m;
                    OnFinalizeTransaction?.Invoke("Card", OriginalTotalDue, OriginalTotalDue);
                }
                else
                {
                    MessageBox.Show("Payment Failed. Please try again.", "Payment Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            paymentWindow.ShowDialog();
        }


        private void ProcessRefund(string chargeId, decimal refundAmount, string transactionId)
        {
            var refundWindow = new CardRefundWindow(chargeId, refundAmount);
            refundWindow.RefundCompleted += (chargeId, refundId, refundAmount) => FinalizeRefundTransaction(chargeId, refundId, refundAmount, transactionId);
            refundWindow.ShowDialog();
        }

        private string PromptForTransactionId()
        {
            TransactionIDInputDialog inputDialog = new TransactionIDInputDialog();
            if (inputDialog.ShowDialog() == true)
            {
                return inputDialog.ResponseText;
            }
            else
            {
                return null;
            }
        }

        private void FinalizeRefundTransaction(string chargeId, string refundId, decimal refundAmount, string transactionId)
        {
            string connectionString = databaseHelper.GetConnectionString();
            string transactionNumber = GetNextTransactionNumber();
            string registerNumber = Properties.Settings.Default.RegisterNumber;
            string locationID = Properties.Settings.Default.LocationID;
            DateTime transactionDate = DateTime.Now;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction dbTransaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertTransactionQuery = @"
                    INSERT INTO Transactions 
                    (TransactionID, TransactionNumber, RegisterNumber, LocationID, TransactionDate, TransactionTime, EmployeeID, TotalAmount, PaymentMethod, ChargeID) 
                    VALUES 
                    (@TransactionID, @TransactionNumber, @RegisterNumber, @LocationID, @TransactionDate, @TransactionTime, @EmployeeID, @TotalAmount, @PaymentMethod, @ChargeID)";

                        using (SqlCommand cmd = new SqlCommand(insertTransactionQuery, connection, dbTransaction))
                        {
                            cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                            cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                            cmd.Parameters.AddWithValue("@RegisterNumber", registerNumber);
                            cmd.Parameters.AddWithValue("@LocationID", locationID);
                            cmd.Parameters.AddWithValue("@TransactionDate", transactionDate.Date);
                            cmd.Parameters.AddWithValue("@TransactionTime", transactionDate.TimeOfDay);
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                            cmd.Parameters.AddWithValue("@TotalAmount", -refundAmount); // Negative for refunds
                            cmd.Parameters.AddWithValue("@PaymentMethod", "Refund");
                            cmd.Parameters.AddWithValue("@ChargeID", chargeId);

                            cmd.ExecuteNonQuery();
                        }

                        dbTransaction.Commit();
                        MessageBox.Show("Refund transaction completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();
                        MessageBox.Show($"Failed to finalize refund: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void UpdateLocationDepositExpected(SqlTransaction dbTransaction, SqlConnection connection, string locationID, decimal netCash)
        {
            string query = @"
        UPDATE LocationBusinessDay
        SET LocationDepositExpected = ISNULL(LocationDepositExpected, 0) + @NetCash
        WHERE BusinessDate = @Today AND LocationID = @LocationID";

            using (SqlCommand cmd = new SqlCommand(query, connection, dbTransaction))
            {
                cmd.Parameters.AddWithValue("@NetCash", netCash);
                cmd.Parameters.AddWithValue("@Today", DateTime.Today);
                cmd.Parameters.AddWithValue("@LocationID", locationID);

                cmd.ExecuteNonQuery();
            }
        }


        private void SaveTransaction(string paymentMethod, decimal totalAmount, decimal paidAmount)
        {
            string transactionNumber = GetNextTransactionNumber();
            string registerNumber = Properties.Settings.Default.RegisterNumber;
            string locationID = Properties.Settings.Default.LocationID;
            bool isTradeHoldLocation = Properties.Settings.Default.LocationIsTradeHold;
            DateTime transactionDate = DateTime.Now;
            decimal commissionAmount = 0m;
            decimal netCash = paymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase) ? paidAmount : 0;

            using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (SqlTransaction dbTransaction = connection.BeginTransaction())
                {
                    try
                    {
                        commissionAmount = CalculateCommissionAmount(connection, dbTransaction);
                        InsertTransaction(dbTransaction, connection, transactionNumber, registerNumber, locationID, transactionDate, paymentMethod, this.subtotal, netCash, chargeId, commissionAmount);

                        if (netCash > 0)
                        {
                            UpdateLocationDepositExpected(dbTransaction, connection, locationID, netCash);
                        }

                        string tradeHoldCartonID = null;

                        var tradeItems = summaryItems
                            .Where(item => item.Type == "Trade")
                            .Select(item => new InventoryItem
                            {
                                SKU = item.SKU,
                                QuantityOnHandSellable = item.Quantity,
                                QuantityOnHandDefective = 0,
                                ProductSerialNumber = item.ProductSerialNumber,
                                ProductName = item.ProductName,
                                LocationID = locationID
                            })
                            .ToList();

                        if (isTradeHoldLocation && tradeItems.Any())
                        {
                            tradeHoldCartonID = EnsureTradeHoldCarton(dbTransaction, connection, locationID, tradeItems);
                        }

                        foreach (var summaryItem in summaryItems)
                        {
                            InsertTransactionDetail(dbTransaction, connection, transactionNumber, summaryItem);

                            // Inventory adjustment
                            if (summaryItem.Type == "Sale")
                            {
                                string updateInventory = @"
                            UPDATE Inventory
                            SET QuantityOnHandSellable = QuantityOnHandSellable - @Quantity
                            WHERE SKU = @SKU AND LocationID = @LocationID";

                                using (SqlCommand cmd = new SqlCommand(updateInventory, connection, dbTransaction))
                                {
                                    cmd.Parameters.AddWithValue("@SKU", summaryItem.SKU);
                                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                                    cmd.Parameters.AddWithValue("@Quantity", summaryItem.Quantity);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else if (summaryItem.Type == "Return")
                            {
                                string updateInventory = @"
                            UPDATE Inventory
                            SET QuantityOnHandSellable = QuantityOnHandSellable + @Quantity
                            WHERE SKU = @SKU AND LocationID = @LocationID";

                                using (SqlCommand cmd = new SqlCommand(updateInventory, connection, dbTransaction))
                                {
                                    cmd.Parameters.AddWithValue("@SKU", summaryItem.SKU);
                                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                                    cmd.Parameters.AddWithValue("@Quantity", summaryItem.Quantity);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // Trades only handled via carton
                            if (summaryItem.Type == "Trade" && isTradeHoldLocation)
                            {
                                AddToTradeHoldCartonDetails(dbTransaction, connection, summaryItem, tradeHoldCartonID, locationID);
                            }
                        }

                        dbTransaction.Commit();
                        PrintReceipt();
                        MessageBox.Show("Transaction completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        dbTransaction.Rollback();
                        MessageBox.Show($"Transaction failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                if (MessageBox.Show("Would the customer like a gift receipt?", "Gift Receipt", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    PrintGiftReceipt();
                }
            }
        }



        private decimal CalculateCommissionAmount(SqlConnection connection, SqlTransaction dbTransaction)
        {
            bool commissionEnabled = Properties.Settings.Default.CommissionEnabled;
            decimal commissionAmount = 0m;

            string employeeQuery = @"
        SELECT EmployeeTipEligible, EmployeeCommissionEligible, EmployeeCommissionRate, EmployeeCommissionLimit
        FROM Employees WHERE EmployeeID = @EmployeeID";

            using (SqlCommand cmd = new SqlCommand(employeeQuery, connection, dbTransaction))
            {
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bool employeeCommissionEligible = reader.GetBoolean(1);
                        decimal commissionRate = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                        decimal commissionLimit = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);

                        decimal salesSubtotal = summaryItems
                            .Where(item => item.Type == "Sale")
                            .Sum(item => item.AdjustedValue != 0 ? item.AdjustedValue * item.Quantity : item.Value * item.Quantity);

                        if (commissionEnabled && employeeCommissionEligible && commissionRate > 0)
                        {
                            commissionAmount = salesSubtotal * (commissionRate / 100);
                            if (commissionLimit > 0 && commissionAmount > commissionLimit)
                            {
                                commissionAmount = commissionLimit;
                            }

                            MessageBox.Show($"Commission calculated: {commissionAmount:C}", "Debug"); // For testing
                        }
                    }
                }
            }

            return commissionAmount;
        }

        private void InsertTransaction(SqlTransaction dbTransaction, SqlConnection connection, string transactionNumber, string registerNumber, string locationID, DateTime transactionDate, string paymentMethod, decimal subtotal, decimal netCash, string chargeId, decimal commissionAmount)
        {
            bool tipsEnabled = Properties.Settings.Default.TipsEnabled;
            bool commissionEnabled = Properties.Settings.Default.CommissionEnabled;

            decimal tipAmount = tipsTotal;
            decimal totalAmount = subtotal + taxes + tipAmount;

            // Insert transaction into Transactions table
            string query = @"
        INSERT INTO Transactions 
        (TransactionID, TransactionNumber, RegisterNumber, LocationID, TransactionDate, TransactionTime, EmployeeID, 
        CustomerID, Subtotal, Taxes, TotalAmount, NetCash, PaymentMethod, ChargeID, TipsTotal, CommissionTotal)
        VALUES 
        (@TransactionID, @TransactionNumber, @RegisterNumber, @LocationID, @TransactionDate, @TransactionTime, @EmployeeID, 
        @CustomerID, @Subtotal, @Taxes, @TotalAmount, @NetCash, @PaymentMethod, @ChargeID, @TipsTotal, @CommissionTotal)";

            using (SqlCommand cmd = new SqlCommand(query, connection, dbTransaction))
            {
                cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                cmd.Parameters.AddWithValue("@RegisterNumber", registerNumber);
                cmd.Parameters.AddWithValue("@LocationID", locationID);
                cmd.Parameters.AddWithValue("@TransactionDate", transactionDate.Date);
                cmd.Parameters.AddWithValue("@TransactionTime", transactionDate.TimeOfDay);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                cmd.Parameters.AddWithValue("@CustomerID", customer?.CustomerID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                cmd.Parameters.AddWithValue("@Taxes", taxes);
                cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                cmd.Parameters.AddWithValue("@NetCash", netCash);
                cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                cmd.Parameters.AddWithValue("@ChargeID", chargeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@TipsTotal", tipAmount);
                cmd.Parameters.AddWithValue("@CommissionTotal", commissionAmount);

                cmd.ExecuteNonQuery();
            }

            // ✅ Ensure LocationTimeCard entry exists — with prompt if not
            string checkTimeCardQuery = @"
        SELECT COUNT(*) 
        FROM LocationTimeCard 
        WHERE EmployeeID = @EmployeeID AND LocationID = @LocationID AND TimePunchDate = @Today";

            using (SqlCommand checkCmd = new SqlCommand(checkTimeCardQuery, connection, dbTransaction))
            {
                checkCmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                checkCmd.Parameters.AddWithValue("@LocationID", locationID);
                checkCmd.Parameters.AddWithValue("@Today", DateTime.Today);

                int existingCount = (int)checkCmd.ExecuteScalar();

                if (existingCount == 0)
                {
                    var result = MessageBox.Show(
                        $"No shift record was found for employee {employeeID} today.\nWould you like to clock in and start a shift now so tips and commission can be recorded?",
                        "Clock In Required",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        string insertTimeCardQuery = @"
                    INSERT INTO LocationTimeCard 
                    (EmployeeID, LocationID, TimePunchDate, TimePunchTime, TimePunchType, TotalShiftTips, TotalShiftCommission)
                    VALUES 
                    (@EmployeeID, @LocationID, @Today, @TimeNow, 'ClockIn', 0, 0)";

                        using (SqlCommand insertCmd = new SqlCommand(insertTimeCardQuery, connection, dbTransaction))
                        {
                            insertCmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                            insertCmd.Parameters.AddWithValue("@LocationID", locationID);
                            insertCmd.Parameters.AddWithValue("@Today", DateTime.Today);
                            insertCmd.Parameters.AddWithValue("@TimeNow", DateTime.Now.TimeOfDay);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // User declined to clock in — do not update tips/commission
                        return;
                    }
                }
            }

            // ✅ Update LocationTimeCard with tips and commission
            string updateTimeCardQuery = @"
        UPDATE LocationTimeCard 
        SET TotalShiftTips = ISNULL(TotalShiftTips, 0) + @TipsTotal,
            TotalShiftCommission = ISNULL(TotalShiftCommission, 0) + @CommissionTotal
        WHERE EmployeeID = @EmployeeID AND LocationID = @LocationID AND TimePunchDate = @Today";

            using (SqlCommand cmd = new SqlCommand(updateTimeCardQuery, connection, dbTransaction))
            {
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                cmd.Parameters.AddWithValue("@LocationID", locationID);
                cmd.Parameters.AddWithValue("@Today", DateTime.Today);
                cmd.Parameters.AddWithValue("@TipsTotal", tipAmount);
                cmd.Parameters.AddWithValue("@CommissionTotal", commissionAmount);
                cmd.ExecuteNonQuery();
            }
        }


        private void InsertTransactionDetail(SqlTransaction dbTransaction, SqlConnection connection, string transactionNumber, SummaryItem item)
        {
            // Check values before inserting
            MessageBox.Show($"Inserting detail - SKU: {item.SKU}, CategoryID: {item.CategoryID}, Price: {item.Price}, DiscountLoyalty: {item.LoyaltyDiscountedValue}, DiscountManual: {item.ManualDiscountedValue}", "Debug");

            string query = @"INSERT INTO TransactionDetails 
                     (TransactionID, TransactionNumber, RegisterNumber, LocationID, TransactionDate, SKU, CategoryID, SerialNumber, Quantity, Price, DiscountLoyalty, DiscountManual, Description)
                     VALUES (@TransactionID, @TransactionNumber, @RegisterNumber, @LocationID, @TransactionDate, @SKU, @CategoryID, @SerialNumber, @Quantity, @Price, @DiscountLoyalty, @DiscountManual, @Description)";

            using (SqlCommand cmd = new SqlCommand(query, connection, dbTransaction))
            {
                cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                cmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
                cmd.Parameters.AddWithValue("@RegisterNumber", Properties.Settings.Default.RegisterNumber);
                cmd.Parameters.AddWithValue("@LocationID", Properties.Settings.Default.LocationID);
                cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now.Date);
                cmd.Parameters.AddWithValue("@SKU", item.SKU);
                cmd.Parameters.AddWithValue("@CategoryID", item.CategoryID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SerialNumber", item.ProductSerialNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                cmd.Parameters.AddWithValue("@Price", item.Price);
                cmd.Parameters.AddWithValue("@DiscountLoyalty", item.LoyaltyDiscountedValue);
                cmd.Parameters.AddWithValue("@DiscountManual", item.ManualDiscountedValue);
                cmd.Parameters.AddWithValue("@Description", item.ProductName);

                cmd.ExecuteNonQuery();
            }
        }

        private string EnsureTradeHoldCarton(SqlTransaction dbTransaction, SqlConnection connection, string locationID, List<InventoryItem> tradeItems)
        {
            string tradeHoldCartonID = null;

            // Check if a carton already exists for TODAY
            string checkCartonQuery = @"
        SELECT TOP 1 TradeHoldCartonID 
        FROM LocationTradeHold 
        WHERE LocationID = @LocationID 
        AND TradeHoldCartonIsFinalized = 0 
        AND CAST(TradeHoldCartonCreationDate AS DATE) = @Today";

            using (SqlCommand cmd = new SqlCommand(checkCartonQuery, connection, dbTransaction))
            {
                cmd.Parameters.AddWithValue("@LocationID", locationID);
                cmd.Parameters.AddWithValue("@Today", DateTime.Today);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    tradeHoldCartonID = result.ToString();
                }
            }

            // If none found, create a new carton
            if (string.IsNullOrEmpty(tradeHoldCartonID))
            {
                tradeHoldCartonID = GenerateTradeHoldCartonID(locationID);
                int tradeHoldDuration = Properties.Settings.Default.LocationTradeHoldDuration;
                DateTime expirationDate = DateTime.Now.AddDays(tradeHoldDuration);

                string insertCartonQuery = @"
            INSERT INTO LocationTradeHold 
            (LocationID, TradeHoldCartonID, TradeHoldCartonExpirationDate, TradeHoldCartonExpectedQuantity, 
             TradeHoldCartonTotalQuantity, TradeHoldCartonTotalDiscrepancyQuantity, 
             TradeHoldCartonIsFinalized, TradeHoldCartonFinalizedEmployeeID, TradeHoldCartonCreationDate)
            VALUES 
            (@LocationID, @TradeHoldCartonID, @TradeHoldCartonExpirationDate, 0, 0, 0, 0, NULL, @Now)";

                using (SqlCommand cmd = new SqlCommand(insertCartonQuery, connection, dbTransaction))
                {
                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                    cmd.Parameters.AddWithValue("@TradeHoldCartonID", tradeHoldCartonID);
                    cmd.Parameters.AddWithValue("@TradeHoldCartonExpirationDate", expirationDate);
                    cmd.Parameters.AddWithValue("@Now", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }

            UpdateTradeHoldCartonDetails(dbTransaction, connection, locationID, tradeHoldCartonID, tradeItems);
            return tradeHoldCartonID;
        }


        private void UpdateTradeHoldCartonDetails(SqlTransaction dbTransaction, SqlConnection connection, string locationID, string tradeHoldCartonID, List<InventoryItem> tradeItems)
        {
            foreach (var item in tradeItems)
            {
                // Check if the SKU already exists in the carton details
                string checkDetailQuery = @"
            SELECT COUNT(*) 
            FROM LocationTradeHoldCartonDetails 
            WHERE LocationID = @LocationID AND LocationTradeHoldCartonID = @TradeHoldCartonID AND SKU = @SKU";

                bool detailExists;
                using (SqlCommand cmd = new SqlCommand(checkDetailQuery, connection, dbTransaction))
                {
                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                    cmd.Parameters.AddWithValue("@TradeHoldCartonID", tradeHoldCartonID);
                    cmd.Parameters.AddWithValue("@SKU", item.SKU);

                    detailExists = (int)cmd.ExecuteScalar() > 0;
                }

                if (detailExists)
                {
                    // Update existing record
                    string updateDetailQuery = @"
                UPDATE LocationTradeHoldCartonDetails
                SET 
                    CartonExpectedQuantitySKUSellable = CartonExpectedQuantitySKUSellable + @ExpectedSellable,
                    CartonExpectedQuantitySKUDefective = CartonExpectedQuantitySKUDefective + @ExpectedDefective
                WHERE LocationID = @LocationID AND LocationTradeHoldCartonID = @TradeHoldCartonID AND SKU = @SKU";

                    using (SqlCommand cmd = new SqlCommand(updateDetailQuery, connection, dbTransaction))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID);
                        cmd.Parameters.AddWithValue("@TradeHoldCartonID", tradeHoldCartonID);
                        cmd.Parameters.AddWithValue("@SKU", item.SKU);
                        cmd.Parameters.AddWithValue("@ExpectedSellable", item.QuantityOnHandSellable);
                        cmd.Parameters.AddWithValue("@ExpectedDefective", item.QuantityOnHandDefective);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Insert new record
                    string insertDetailQuery = @"
                INSERT INTO LocationTradeHoldCartonDetails 
                (LocationID, LocationTradeHoldCartonID, SKU, CartonExpectedQuantitySKUSellable, CartonExpectedQuantitySKUDefective, CartonActualQuantitySKUSellable, CartonActualQuantitySKUDefective, CartonDiscrepancySKUSellable, CartonDiscrepancySKUDefective)
                VALUES 
                (@LocationID, @TradeHoldCartonID, @SKU, @ExpectedSellable, @ExpectedDefective, 0, 0, 0, 0)";

                    using (SqlCommand cmd = new SqlCommand(insertDetailQuery, connection, dbTransaction))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID);
                        cmd.Parameters.AddWithValue("@TradeHoldCartonID", tradeHoldCartonID);
                        cmd.Parameters.AddWithValue("@SKU", item.SKU);
                        cmd.Parameters.AddWithValue("@ExpectedSellable", item.QuantityOnHandSellable);
                        cmd.Parameters.AddWithValue("@ExpectedDefective", item.QuantityOnHandDefective);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private string GenerateTradeHoldCartonID(string locationID)
        {
            return $"{locationID}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private void AddToTradeHoldCartonDetails(SqlTransaction dbTransaction, SqlConnection connection, SummaryItem item, string tradeHoldCartonID, string locationID)
        {
            // Check if a record already exists for the SKU in the carton
            string selectQuery = @"SELECT TOP 1 * 
                           FROM LocationTradeHoldCartonDetails 
                           WHERE LocationTradeHoldCartonID = @TradeHoldCartonID 
                           AND LocationID = @LocationID";

            bool recordExists = false;

            using (SqlCommand cmd = new SqlCommand(selectQuery, connection, dbTransaction))
            {
                cmd.Parameters.AddWithValue("@TradeHoldCartonID", tradeHoldCartonID);
                cmd.Parameters.AddWithValue("@LocationID", locationID);
                recordExists = cmd.ExecuteScalar() != null;
            }

            if (!recordExists)
            {
                // Insert the traded item into the LocationTradeHoldCartonDetails table
                string insertQuery = @"INSERT INTO LocationTradeHoldCartonDetails (LocationID, LocationTradeHoldCartonID, CartonExpectedQuantitySKUSellable, CartonExpectedQuantitySKUDefective, CartonActualQuantitySKUSellable, CartonActualQuantitySKUDefective, CartonDiscrepancySKUSellable, CartonDiscrepancySKUDefective)
                               VALUES (@LocationID, @TradeHoldCartonID, @ExpectedQuantitySellable, @ExpectedQuantityDefective, 0, 0, 0, 0)";

                using (SqlCommand cmd = new SqlCommand(insertQuery, connection, dbTransaction))
                {
                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                    cmd.Parameters.AddWithValue("@TradeHoldCartonID", tradeHoldCartonID);
                    cmd.Parameters.AddWithValue("@ExpectedQuantitySellable", item.Quantity); // Sellable quantity for now
                    cmd.Parameters.AddWithValue("@ExpectedQuantityDefective", 0); // Default to 0 for defective items

                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Update the existing record with the new quantities
                string updateQuery = @"UPDATE LocationTradeHoldCartonDetails
                               SET CartonExpectedQuantitySKUSellable = CartonExpectedQuantitySKUSellable + @AdditionalQuantitySellable
                               WHERE LocationID = @LocationID AND LocationTradeHoldCartonID = @TradeHoldCartonID";

                using (SqlCommand cmd = new SqlCommand(updateQuery, connection, dbTransaction))
                {
                    cmd.Parameters.AddWithValue("@AdditionalQuantitySellable", item.Quantity);
                    cmd.Parameters.AddWithValue("@LocationID", locationID);
                    cmd.Parameters.AddWithValue("@TradeHoldCartonID", tradeHoldCartonID);

                    cmd.ExecuteNonQuery();
                }
            }

            // Update the overall trade hold carton quantities
            string updateCartonQuery = @"UPDATE LocationTradeHold 
                                 SET TradeHoldCartonExpectedQuantity = TradeHoldCartonExpectedQuantity + @AdditionalQuantity
                                 WHERE TradeHoldCartonID = @TradeHoldCartonID AND LocationID = @LocationID";

            using (SqlCommand cmd = new SqlCommand(updateCartonQuery, connection, dbTransaction))
            {
                cmd.Parameters.AddWithValue("@AdditionalQuantity", item.Quantity);
                cmd.Parameters.AddWithValue("@TradeHoldCartonID", tradeHoldCartonID);
                cmd.Parameters.AddWithValue("@LocationID", locationID);

                cmd.ExecuteNonQuery();
            }
        }


        private decimal CalculateTotalManualDiscount()
        {
            return summaryItems
                .Where(item => item.IsManualDiscount)
                .Sum(item => item.ManualDiscountedValue * item.Quantity);
        }

        private string GetNextTransactionNumber()
        {
            string nextTransactionNumber = "1";
            string query = "SELECT MAX(TransactionNumber) FROM Transactions WHERE TransactionDate = @TransactionDate";

            using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TransactionDate", DateTime.Now.Date);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        nextTransactionNumber = (Convert.ToInt32(result) + 1).ToString();
                    }
                }
            }

            return nextTransactionNumber;
        }

        private string GenerateTransactionId()
        {
            return Guid.NewGuid().ToString();
        }

        private string FetchChargeId(string merlinId)
        {
            string connectionString = databaseHelper.GetConnectionString();
            string chargeId = null;

            string query = "SELECT ChargeID FROM Transactions WHERE MerlinID = @MerlinID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MerlinID", merlinId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            chargeId = reader["ChargeID"] as string;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(chargeId))
            {
                MessageBox.Show($"No charge record found for MerlinID: {merlinId}", "No Record", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return chargeId;
        }


        private void BtnOnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(null);
        }


        public void StorePaymentDetails(string cardBrand, string cardLastFour, int expMonth, int expYear, string chargeId)
        {
            this.cardBrand = cardBrand;
            this.cardLastFour = cardLastFour;
            this.expMonth = expMonth;
            this.expYear = expYear;
            this.paymentMethod = "Card";
            this.chargeId = chargeId;
        }

        public string GetCardBrand() { return cardBrand; }
        public string GetCardLastFour() { return cardLastFour; }
        public int GetExpMonth() { return expMonth; }
        public int GetExpYear() { return expYear; }
        public string GetPaymentMethod() { return paymentMethod; }
        public string GetLastChargeId() { return chargeId; }





        public void PrintReceipt()
        {
            PrintDocument printDocument = new PrintDocument();

            // Specify the name of the Epson receipt printer
            string receiptPrinterName = "EPSON TM-T88V Receipt"; // Replace with your actual printer name
            string szPrinterName = receiptPrinterName;

            // Check if the specified printer exists among installed printers
            if (PrinterSettings.InstalledPrinters.Cast<string>().Any(name => name == receiptPrinterName))
            {
                printDocument.PrinterSettings.PrinterName = receiptPrinterName;

                // Assign the PrintPage event handler
                printDocument.PrintPage += new PrintPageEventHandler(PrintPage);

                // Print the document
                printDocument.Print();

                // If payment method is cash, open the drawer
                if (paymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] drawerOpenCommand = new byte[] { 27, 112, 0, 25, 250 };

                    // Send command to open the drawer
                    RawPrinterHelper.SendBytesToPrinter(szPrinterName, drawerOpenCommand);
                }
            }
            else
            {
                MessageBox.Show($"Printer '{receiptPrinterName}' not found.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Font font = new Font("Inter", 10);
            Font boldFont = new Font("Inter", 10, System.Drawing.FontStyle.Bold);
            string transactionNumberString = transactionNumber.ToString();


            float fontHeight = font.GetHeight();
            int startX = 10;
            int receiptWidth = 270; // Approximate width in pixels for 80mm at 96 DPI, adjust as needed
            int offset = 10; // Initial offset for drawing content
            int columnOffset = 165; // Offset for the second column (for time, store, register)


            // Optimize and print the business logo
            Bitmap optimizedLogo = OptimizeImageForPrinter(Properties.Resources.NGP_Receipt_Logo_WhiteBG);
            graphics.DrawImage(optimizedLogo, startX, offset, receiptWidth, optimizedLogo.Height);
            offset += optimizedLogo.Height + 10;
            DelayForPrinter();

            // Top Section: Employee and Location Details
            // First column
            graphics.DrawString($"Date: {DateTime.Now.ToShortDateString()}", font, Brushes.Black, startX, offset);
            graphics.DrawString($"Transaction: # {transactionNumberString}", font, Brushes.Black, startX, offset + 20);
            graphics.DrawString($"Associate: {employeeID}", font, Brushes.Black, startX, offset + 40);

            // Second column
            graphics.DrawString($"Time: {DateTime.Now.ToShortTimeString()}", font, Brushes.Black, startX + columnOffset, offset);
            graphics.DrawString($"Store: {Settings.Default.LocationID}", font, Brushes.Black, startX + columnOffset, offset + 20);
            graphics.DrawString($"Register: {Settings.Default.RegisterNumber}", font, Brushes.Black, startX + columnOffset, offset + 40);

            offset += 60; // Adjust offset based on content height


            // Separator line
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
            offset += (int)fontHeight;

            // Print sections
            PrintSection(graphics, font, boldFont, startX, ref offset, receiptWidth, "Sales", summaryItems.Where(i => i.Type == "Sale"));
            PrintSection(graphics, font, boldFont, startX, ref offset, receiptWidth, "Returns", summaryItems.Where(i => i.Type == "Return"));
            PrintSection(graphics, font, boldFont, startX, ref offset, receiptWidth, "Trades", summaryItems.Where(i => i.Type == "Trade"));

            // Print totals and payment info
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset); // Separator line
            PrintTotals(graphics, font, boldFont, startX, ref offset, receiptWidth);
            offset += (int)fontHeight;

            PrintPaymentInfo(graphics, font, boldFont, startX, ref offset, receiptWidth);
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset); // Separator line
            offset += (int)fontHeight;

            // QR Code
            Bitmap qrCodeImage = GenerateQRCode(transactionId);
            graphics.DrawImage(qrCodeImage, startX + (receiptWidth - qrCodeImage.Width) / 2, offset);
            offset += qrCodeImage.Height + 10;
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset); // Separator line
            offset += (int)fontHeight;

            // Loyalty Program Logo and Message
            Bitmap optimizedPerksLogo = OptimizeImageForPrinter(Properties.Resources.PlusPerks_Receipt_Logo_Black);
            int logoMaxWidth = receiptWidth - 20; // Add padding
            float scaleFactor = Math.Min(0.75f, logoMaxWidth / (float)optimizedPerksLogo.Width); // Scale to fit
            int scaledLogoWidth = (int)(optimizedPerksLogo.Width * scaleFactor);
            int scaledLogoHeight = (int)(optimizedPerksLogo.Height * scaleFactor);
            graphics.DrawImage(optimizedPerksLogo, startX + (receiptWidth - scaledLogoWidth) / 2, offset, scaledLogoWidth, scaledLogoHeight);
            offset += scaledLogoHeight + 10;
            DelayForPrinter();

            string loyaltyAdTitle = "Join PlusPerks Today!";
            string loyaltyAdText = "Earn points on every purchase, save on pre-owned items, and more! Ask us how to enroll.";

            graphics.DrawString(loyaltyAdTitle, boldFont, Brushes.Black, startX + (receiptWidth - graphics.MeasureString(loyaltyAdTitle, boldFont).Width) / 2, offset);
            offset += (int)boldFont.GetHeight() + 5;

            List<string> wrappedTextLines = WrapText(loyaltyAdText, receiptWidth, font, graphics);
            foreach (var line in wrappedTextLines)
            {
                graphics.DrawString(line, font, Brushes.Black, startX + (receiptWidth - graphics.MeasureString(line, font).Width) / 2, offset);
                offset += (int)fontHeight + 2;
            }

        }

        private void DelayForPrinter()
        {
            System.Threading.Thread.Sleep(100); // Adjust based on printer responsiveness
        }



        private void PrintSection(Graphics graphics, Font font, Font boldFont, int startX, ref int offset, int receiptWidth, string sectionName, IEnumerable<SummaryItem> items)
        {
            if (items.Any())
            {
                graphics.DrawString($"{sectionName}:", boldFont, new SolidBrush(System.Drawing.Color.Black), startX, offset);
                offset += (int)font.GetHeight() + 5; // Add space after section title

                foreach (var item in items)
                {
                    string itemTitle = item.ProductName.Length > 34 ? item.ProductName.Substring(0, 34) + "..." : item.ProductName;
                    string itemSKU = $"SKU: {item.SKU}";
                    decimal itemPrice = item.AdjustedValue != 0 ? item.AdjustedValue : item.Value;
                    string itemTotalText = $"{itemPrice:C2}";

                    graphics.DrawString(itemTitle, font, new SolidBrush(System.Drawing.Color.Black), startX, offset);
                    offset += (int)font.GetHeight() + 5;
                    graphics.DrawString(itemSKU, font, new SolidBrush(System.Drawing.Color.Black), startX, offset);

                    // Right-align item total
                    SizeF itemTotalSize = graphics.MeasureString(itemTotalText, font);
                    int itemTotalPositionX = receiptWidth - (int)itemTotalSize.Width - 10;
                    graphics.DrawString(itemTotalText, font, new SolidBrush(System.Drawing.Color.Black), itemTotalPositionX, offset);
                    offset += (int)font.GetHeight() + 10;
                }
            }
        }

        private void PrintTotals(Graphics graphics, Font font, Font boldFont, int startX, ref int offset, int receiptWidth)
        {
            offset += 10;
            graphics.DrawString("----------------------------------------", font, System.Drawing.Brushes.Black, startX, offset);
            offset += (int)boldFont.GetHeight() + 5;

            string[] labels = { "Subtotal:", "Sales Tax:", "Discounts:", "Tip:", "Total:" };
            decimal[] amounts = { subtotal, taxes, discounts, tipsTotal, TotalDue };

            for (int i = 0; i < labels.Length; i++)
            {
                string label = labels[i];
                string amountText = $"{amounts[i]:C2}";
                SizeF amountSize = graphics.MeasureString(amountText, boldFont);
                int amountPosX = receiptWidth - (int)amountSize.Width - 10 + startX;

                graphics.DrawString(label, boldFont, System.Drawing.Brushes.Black, startX, offset);
                graphics.DrawString(amountText, boldFont, System.Drawing.Brushes.Black, amountPosX, offset);
                offset += (int)boldFont.GetHeight() + 5;
            }
        }

        private void PrintPaymentInfo(Graphics graphics, Font font, Font boldFont, int startX, ref int offset, int receiptWidth)
        {
            offset += 10;
            graphics.DrawString("----------------------------------------", font, new SolidBrush(System.Drawing.Color.Black), startX, offset);
            offset += (int)font.GetHeight() + 5;

            string paymentInfoTitle = TotalDue < 0 ? "Refund Details:" : "Payment Details:";
            graphics.DrawString(paymentInfoTitle, boldFont, new SolidBrush(System.Drawing.Color.Black), startX, offset);
            offset += (int)font.GetHeight() + 5;

            if (TotalDue < 0)
            {
                graphics.DrawString($"Amount Refunded: {Math.Abs(TotalDue):C2}", font, new SolidBrush(System.Drawing.Color.Black), startX, offset);
            }
            else
            {
                graphics.DrawString($"Total Paid: {TotalDue:C2}", font, new SolidBrush(System.Drawing.Color.Black), startX, offset);
            }
            offset += (int)font.GetHeight() + 5;

            if (paymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
            {
                decimal changeGiven = TotalPaidInCash - TotalDue;
                string[] paymentLabels = { "Amount Tendered:", "Change:" };
                decimal[] paymentAmounts = { TotalPaidInCash, changeGiven };

                for (int i = 0; i < paymentLabels.Length; i++)
                {
                    string label = paymentLabels[i];
                    string amountText = $"{paymentAmounts[i]:C2}";

                    SizeF amountSize = graphics.MeasureString(amountText, font);
                    int amountPosX = receiptWidth - (int)amountSize.Width - 10 + startX;

                    graphics.DrawString(label, font, new SolidBrush(System.Drawing.Color.Black), startX, offset);
                    graphics.DrawString(amountText, font, new SolidBrush(System.Drawing.Color.Black), amountPosX, offset);
                    offset += (int)font.GetHeight() + 5;
                }
            }

            if (paymentMethod.Equals("Card", StringComparison.OrdinalIgnoreCase))
            {
                string formattedCardBrand = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cardBrand.ToLower());
                string cardInfo = $"Card: **** **** **** {cardLastFour}\nBrand: {formattedCardBrand}\nExp: {expMonth}/{expYear}";
                graphics.DrawString(cardInfo, font, new SolidBrush(System.Drawing.Color.Black), startX, offset);
                offset += (int)(3 * font.GetHeight() + 5);
            }
        }

        private void PrintBarcodeSection(Graphics graphics, Font font, Font boldFont, int startX, ref int offset, int receiptWidth)
        {
            graphics.DrawString("----------------------------------------", font, System.Drawing.Brushes.Black, startX, offset);
            offset += (int)font.GetHeight() + 10;

            if (string.IsNullOrEmpty(transactionId))
            {
                GenerateTransactionId(); // Ensure merlinID is set
            }

            try
            {
                string transactionId = this.transactionId;
                Bitmap qrCodeImage = GenerateQRCode(transactionId);
                int qrCodeHeight = qrCodeImage.Height;
                int centeredX = startX + (receiptWidth - qrCodeImage.Width) / 2;

                graphics.DrawImage(qrCodeImage, centeredX, offset);
                offset += qrCodeHeight - 5;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to generate or print QR code: " + ex.Message);
            }
        }

        private void PrintCustomerThanks(Graphics graphics, Font font, Font boldFont, int startX, ref int offset, int receiptWidth)
        {
            string customerName = customer != null
                ? $"{customer.CustomerFirstName} {customer.CustomerLastName}".Trim()
                : "Valued Customer";

            string loyaltyStatus = customer != null && customer.CustomerLoyalty
                ? "Loyalty Member"
                : "Non-Member";

            if (!string.IsNullOrEmpty(loyaltyStatus) && loyaltyStatus != "Non-Member")
            {
                string thankYouMessage = $"Thanks for shopping at NewGamePlus,";
                string customerNameMessage = $"{customerName}!";
                string loyaltyStatusMessage = $"Membership Status: {loyaltyStatus}";

                offset += 20;
                graphics.DrawString("----------------------------------------", font, new SolidBrush(System.Drawing.Color.Black), startX, offset);
                offset += (int)font.GetHeight() + 5;

                SizeF thankYouSize = graphics.MeasureString(thankYouMessage, font);
                graphics.DrawString(thankYouMessage, font, new SolidBrush(System.Drawing.Color.Black), startX + (receiptWidth - (int)thankYouSize.Width) / 2, offset);
                offset += (int)font.GetHeight() + 5;

                SizeF customerNameSize = graphics.MeasureString(customerNameMessage, boldFont);
                graphics.DrawString(customerNameMessage, boldFont, new SolidBrush(System.Drawing.Color.Black), startX + (receiptWidth - (int)customerNameSize.Width) / 2, offset);
                offset += (int)boldFont.GetHeight() + 5;

                SizeF loyaltyStatusSize = graphics.MeasureString(loyaltyStatusMessage, font);
                graphics.DrawString(loyaltyStatusMessage, font, new SolidBrush(System.Drawing.Color.Black), startX + (receiptWidth - (int)loyaltyStatusSize.Width) / 2, offset);
                offset += (int)font.GetHeight() + 5;
            }
            else
            {
                offset += 20;
                graphics.DrawString("----------------------------------------", font, new SolidBrush(System.Drawing.Color.Black), startX, offset);
                offset += (int)font.GetHeight() + 5;

                string loyaltyAdTitle = "Join PlusPerks Today!";
                string loyaltyAdText = "Earn points on every purchase, save on pre-owned items, get bonus trade credit, and more! Ask us how to enroll.";

                System.Drawing.Image plusPerksLogo = Properties.Resources.PlusPerks_Receipt_Logo_Black;
                float logoScaleFactor = 0.085f;
                int scaledLogoWidth = (int)(plusPerksLogo.Width * logoScaleFactor);
                int scaledLogoHeight = (int)(plusPerksLogo.Height * logoScaleFactor);

                graphics.DrawImage(plusPerksLogo, startX + (receiptWidth - scaledLogoWidth) / 5, offset, scaledLogoWidth, scaledLogoHeight);
                offset += scaledLogoHeight + 10;

                SizeF loyaltyAdTitleSize = graphics.MeasureString(loyaltyAdTitle, boldFont);
                graphics.DrawString(loyaltyAdTitle, boldFont, new SolidBrush(System.Drawing.Color.Black), (receiptWidth - loyaltyAdTitleSize.Width) / 2 + startX, offset);
                offset += (int)boldFont.GetHeight() + 5;

                List<string> wrappedTextLines = WrapText(loyaltyAdText, receiptWidth, font, graphics);
                foreach (var line in wrappedTextLines)
                {
                    SizeF lineSize = graphics.MeasureString(line, font);
                    graphics.DrawString(line, font, new SolidBrush(System.Drawing.Color.Black), startX + (receiptWidth - lineSize.Width) / 2, offset);
                    offset += (int)font.GetHeight() + 2;
                }
            }

            System.Drawing.Image paymentMethodsReceiptImage = Properties.Resources.PaymentMethods;
            System.Drawing.Image receiptBase = Properties.Resources.ReceiptBase;

            float paymentsScaleFactor = 0.115f;
            int paymentsWidth = (int)(paymentMethodsReceiptImage.Width * paymentsScaleFactor);
            int paymentsHeight = (int)(paymentMethodsReceiptImage.Height * paymentsScaleFactor);

            graphics.DrawImage(paymentMethodsReceiptImage, startX + (receiptWidth - paymentsWidth) / 2, offset, paymentsWidth, paymentsHeight);

            int bottomWhiteSpace = 5000;
            offset += bottomWhiteSpace;
        }

        private void PrintGiftReceipt()
        {
            var dialog = new GiftReceiptItemSelectionDialog(summaryItems.Where(i => i.Type == "Sale").ToList());
            if (dialog.ShowDialog() == true && dialog.SelectedItems.Any())
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = "EPSON TM-T88V Receipt";

                printDocument.PrintPage += (sender, args) =>
                {
                    PrintGiftReceiptPage(args.Graphics, dialog.SelectedItems);
                };

                printDocument.Print();
            }
            else
            {
                MessageBox.Show("No items selected for the gift receipt.", "Gift Receipt", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        private void PrintGiftReceiptPage(Graphics graphics, List<SummaryItem> selectedItems)
        {
            Font font = new Font("Inter", 10);
            Font boldFont = new Font("Inter", 10, System.Drawing.FontStyle.Bold);

            float fontHeight = font.GetHeight();
            int startX = 10;
            int receiptWidth = 270; // Approximate receipt width
            int offset = 10;

            // Optimize and print the business logo
            Bitmap optimizedLogo = OptimizeImageForPrinter(Properties.Resources.NGP_Receipt_Logo_WhiteBG);
            graphics.DrawImage(optimizedLogo, startX, offset, receiptWidth, optimizedLogo.Height);
            offset += optimizedLogo.Height + 10;
            DelayForPrinter();

            // Separator line
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
            offset += (int)fontHeight;

            // Transaction details
            graphics.DrawString($"Transaction: #{transactionNumber}", font, Brushes.Black, startX, offset);
            offset += (int)fontHeight + 5;

            // Gift receipt header
            graphics.DrawString("Gift Receipt Items:", boldFont, Brushes.Black, startX, offset);
            offset += (int)fontHeight + 5;

            // Print selected items
            foreach (var item in selectedItems)
            {
                string itemName = item.ProductName.Length > 34 ? item.ProductName.Substring(0, 34) + "..." : item.ProductName;
                graphics.DrawString(itemName, font, Brushes.Black, startX, offset);
                offset += (int)fontHeight + 5;
            }

            // Separator line
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
            offset += (int)fontHeight;

            // QR Code for TransactionID
            Bitmap qrCodeImage = GenerateQRCode(transactionId);
            int qrCodeHeight = qrCodeImage.Height;
            int centeredX = startX + (receiptWidth - qrCodeImage.Width) / 2;
            graphics.DrawImage(qrCodeImage, centeredX, offset);
            offset += qrCodeHeight + 10;
            DelayForPrinter();

            // PlusPerks logo and message
            Bitmap optimizedPerksLogo = OptimizeImageForPrinter(Properties.Resources.PlusPerks_Receipt_Logo_Black);
            int logoMaxWidth = receiptWidth - 40;
            float scaleFactor = Math.Min(0.65f, logoMaxWidth / (float)optimizedPerksLogo.Width);
            int scaledLogoWidth = (int)(optimizedPerksLogo.Width * scaleFactor);
            int scaledLogoHeight = (int)(optimizedPerksLogo.Height * scaleFactor);
            graphics.DrawImage(optimizedPerksLogo, startX + (receiptWidth - scaledLogoWidth) / 2, offset, scaledLogoWidth, scaledLogoHeight);
            offset += scaledLogoHeight + 10;

            string loyaltyAdTitle = "Join PlusPerks Today!";
            string loyaltyAdText = "Earn points on every purchase, save on pre-owned items, and more! Ask us how to enroll.";

            graphics.DrawString(loyaltyAdTitle, boldFont, Brushes.Black, startX + (receiptWidth - graphics.MeasureString(loyaltyAdTitle, boldFont).Width) / 2, offset);
            offset += (int)boldFont.GetHeight() + 5;

            List<string> wrappedTextLines = WrapText(loyaltyAdText, receiptWidth, font, graphics);
            foreach (var line in wrappedTextLines)
            {
                graphics.DrawString(line, font, Brushes.Black, startX + (receiptWidth - graphics.MeasureString(line, font).Width) / 2, offset);
                offset += (int)fontHeight + 2;
            }

            // Final separator
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
            offset += (int)fontHeight;
        }




        private List<string> WrapText(string text, int pixels, Font font, Graphics graphics)
        {
            List<string> wrappedLines = new List<string>();
            string[] words = text.Split(' ');
            string currentLine = "";

            foreach (var word in words)
            {
                var testLine = currentLine.Length == 0 ? word : $"{currentLine} {word}";
                var testLineWidth = graphics.MeasureString(testLine, font).Width;

                if (testLineWidth > pixels)
                {
                    wrappedLines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (currentLine.Length > 0)
            {
                wrappedLines.Add(currentLine);
            }

            return wrappedLines;
        }

        private Bitmap OptimizeImageForPrinter(Bitmap original)
        {
            int targetWidth = 270; // Adjust for receipt width
            int targetHeight = (int)(original.Height * (targetWidth / (float)original.Width));
            return new Bitmap(original, new System.Drawing.Size(targetWidth, targetHeight));
        }


        private Bitmap GenerateQRCode(string content)
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = 100,
                    Width = 100,
                    Margin = 1  // Adjust the margin as needed
                },
                Renderer = new BitmapRenderer()

            };
            return writer.Write(content);
        }

        private void BtnPrintBill_Click(object sender, RoutedEventArgs e)
        {
            PrintTipReceipt();
        }

        private void PrintTipReceipt()
        {
            PrintDocument printDocument = new PrintDocument();
            string receiptPrinterName = "EPSON TM-T88V Receipt"; // Adjust as needed

            if (PrinterSettings.InstalledPrinters.Cast<string>().Any(name => name == receiptPrinterName))
            {
                printDocument.PrinterSettings.PrinterName = receiptPrinterName;
                printDocument.PrintPage += (sender, e) => PrintTipReceiptPage(e.Graphics);
                printDocument.Print();
            }
            else
            {
                MessageBox.Show($"Printer '{receiptPrinterName}' not found.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void PrintTipReceiptPage(Graphics graphics)
        {
            Font font = new Font("Inter", 10);
            Font signatureLabelFont = new Font("Inter", 8);
            Font boldFont = new Font("Inter", 10, System.Drawing.FontStyle.Bold);
            int startX = 10;
            int receiptWidth = 270;
            int offset = 10;

            // Draw logo
            Bitmap optimizedLogo = OptimizeImageForPrinter(Properties.Resources.NGP_Receipt_Logo_WhiteBG);
            graphics.DrawImage(optimizedLogo, startX, offset, receiptWidth, optimizedLogo.Height);
            offset += optimizedLogo.Height + 10;
            DelayForPrinter();

            // Header info
            graphics.DrawString($"Date: {DateTime.Now.ToShortDateString()}", font, Brushes.Black, startX, offset);
            graphics.DrawString($"Time: {DateTime.Now.ToShortTimeString()}", font, Brushes.Black, startX + 160, offset);
            offset += 20;

            graphics.DrawString($"Associate: {employeeID}", font, Brushes.Black, startX, offset);
            graphics.DrawString($"Register: {Settings.Default.RegisterNumber}", font, Brushes.Black, startX + 160, offset);
            offset += 20;

            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
            offset += 20;

            // Sales Items
            PrintSection(graphics, font, boldFont, startX, ref offset, receiptWidth, "Sales", summaryItems.Where(i => i.Type == "Sale"));

            // Totals
            PrintBillTotals(graphics, font, boldFont, startX, ref offset, receiptWidth);
            offset += 30;

            // Tip & Total Lines (right-aligned)
            string tipLabel = "Tip:                                       $ ______.______";
            string totalLabel = "Total:                                   $ ______.______";

            SizeF tipSize = graphics.MeasureString(tipLabel, boldFont);
            graphics.DrawString(tipLabel, boldFont, Brushes.Black, startX + receiptWidth - (int)tipSize.Width - 10, offset);
            offset += 40;

            SizeF totalSize = graphics.MeasureString(totalLabel, boldFont);
            graphics.DrawString(totalLabel, boldFont, Brushes.Black, startX + receiptWidth - (int)totalSize.Width - 10, offset);
            offset += 65;

            // Full-width signature line
            graphics.DrawString("X ________________________________________________", boldFont, Brushes.Black, startX, offset);
            offset += 15;

            string sigLabel = "Customer Signature";
            offset += 5;
            SizeF sigSize = graphics.MeasureString(sigLabel, signatureLabelFont);
            graphics.DrawString(sigLabel, font, Brushes.Black, startX + (receiptWidth - sigSize.Width) / 2, offset);
            offset += 40;

           
            graphics.DrawString(new string('-', 40), font, Brushes.Black, startX, offset);
            offset += 20;

            // Optional QR code
            Bitmap qr = GenerateQRCode(transactionId);
            graphics.DrawImage(qr, startX + (receiptWidth - qr.Width) / 2, offset);
            offset += qr.Height + 20;
        }

        private void PrintBillTotals(Graphics graphics, Font font, Font boldFont, int startX, ref int offset, int receiptWidth)
        {
            offset += 10;
            graphics.DrawString("----------------------------------------", font, System.Drawing.Brushes.Black, startX, offset);
            offset += (int)boldFont.GetHeight() + 5;

            string[] labels = { "Subtotal:", "Sales Tax:", "Total:" };
            decimal[] amounts = { subtotal, taxes, TotalDue };

            for (int i = 0; i < labels.Length; i++)
            {
                string label = labels[i];
                string amountText = $"{amounts[i]:C2}";
                SizeF amountSize = graphics.MeasureString(amountText, boldFont);
                int amountPosX = receiptWidth - (int)amountSize.Width - 10 + startX;

                graphics.DrawString(label, boldFont, System.Drawing.Brushes.Black, startX, offset);
                graphics.DrawString(amountText, boldFont, System.Drawing.Brushes.Black, amountPosX, offset);
                offset += (int)boldFont.GetHeight() + 5;
            }
        }


    }
}