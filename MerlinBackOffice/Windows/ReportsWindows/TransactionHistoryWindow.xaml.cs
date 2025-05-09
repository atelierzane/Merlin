using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinBackOffice.Helpers;
using MerlinBackOffice.Models;

namespace MerlinBackOffice.Windows.ReportsWindows
{
    public partial class TransactionHistoryWindow : Window
    {
        private ObservableCollection<Transaction> transactions;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();

        public TransactionHistoryWindow()
        {
            InitializeComponent();
            LoadRegisters();
            LoadTransactions();
        }

        private void LoadRegisters()
        {
            // Populate register combo box
            string query = "SELECT DISTINCT RegisterNumber FROM Transactions WHERE LocationID = @LocationID";
            try
            {
                using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", Properties.Settings.Default.LocationID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cbRegister.Items.Add(reader["RegisterNumber"].ToString());
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading registers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTransactions(DateTime? selectedDate = null, string registerNumber = null, int? transactionNumber = null, string merlinID = null)
        {
            transactions = new ObservableCollection<Transaction>();

            string query = @"
        SELECT 
            t.TransactionNumber,
            t.TransactionDate,
            t.TransactionTime,
            t.EmployeeID,
            t.CustomerID,
            t.Subtotal,
            t.Taxes,
            t.Fees,
            (ISNULL(t.DiscountsLoyalty, 0) + ISNULL(t.DiscountsManual, 0)) AS Discounts,
            t.TotalAmount,
            t.PaymentMethod
        FROM Transactions t
        WHERE t.LocationID = @LocationID";

            // Add filters dynamically
            if (selectedDate != null)
                query += " AND t.TransactionDate = @TransactionDate";

            if (!string.IsNullOrEmpty(registerNumber))
                query += " AND t.RegisterNumber = @RegisterNumber";

            if (transactionNumber != null)
                query += " AND t.TransactionNumber = @TransactionNumber";

            if (!string.IsNullOrEmpty(merlinID))
                query += " AND t.TransactionID = @MerlinID";

            query += " ORDER BY t.TransactionDate DESC, t.TransactionTime DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", Properties.Settings.Default.LocationID);

                        if (selectedDate != null)
                            command.Parameters.AddWithValue("@TransactionDate", selectedDate.Value.Date);

                        if (!string.IsNullOrEmpty(registerNumber))
                            command.Parameters.AddWithValue("@RegisterNumber", registerNumber);

                        if (transactionNumber != null)
                            command.Parameters.AddWithValue("@TransactionNumber", transactionNumber.Value);

                        if (!string.IsNullOrEmpty(merlinID))
                            command.Parameters.AddWithValue("@MerlinID", merlinID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    TransactionNumber = reader["TransactionNumber"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TransactionNumber"]),
                                    TransactionDate = reader["TransactionDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["TransactionDate"]),
                                    TransactionTime = reader["TransactionTime"] == DBNull.Value ? DateTime.MinValue : DateTime.Parse(reader["TransactionTime"].ToString()),
                                    EmployeeID = reader["EmployeeID"] == DBNull.Value ? "Unknown" : reader["EmployeeID"].ToString(),
                                    CustomerID = reader["CustomerID"] == DBNull.Value ? "N/A" : reader["CustomerID"].ToString(),
                                    Subtotal = reader["Subtotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Subtotal"]),
                                    Taxes = reader["Taxes"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Taxes"]),
                                    Fees = reader["Fees"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Fees"]),
                                    Discounts = reader["Discounts"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Discounts"]),
                                    TotalAmount = reader["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalAmount"]),
                                    PaymentMethod = reader["PaymentMethod"] == DBNull.Value ? "N/A" : reader["PaymentMethod"].ToString()
                                });
                            }
                        }
                    }
                }

                lvTransactions.ItemsSource = transactions;

                if (transactions.Count == 0)
                {
                    MessageBox.Show("No transactions found for the selected date.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidCastException ex)
            {
                MessageBox.Show($"Data conversion error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? selectedDate = calendar.SelectedDate;
            string registerNumber = cbRegister.SelectedItem?.ToString();
            LoadTransactions(selectedDate, registerNumber);
        }

        private void JumpToTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tbTransactionNumber.Text.Trim(), out int transactionNumber))
            {
                LoadTransactions(transactionNumber: transactionNumber);
            }
            else
            {
                MessageBox.Show("Please enter a valid transaction number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchByMerlinID_Click(object sender, RoutedEventArgs e)
        {
            string merlinID = txtMerlinID.Text.Trim();
            if (!string.IsNullOrEmpty(merlinID))
            {
                LoadTransactions(merlinID: merlinID);
            }
            else
            {
                MessageBox.Show("Please enter a valid Merlin ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PrintJournalForDate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature not implemented yet.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReprintReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (lvTransactions.SelectedItem is Transaction selectedTransaction)
            {
                MessageBox.Show($"Reprinting receipt for transaction {selectedTransaction.TransactionNumber}.", "Reprint Receipt", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a transaction to reprint the receipt.", "Invalid Action", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature not implemented yet.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
