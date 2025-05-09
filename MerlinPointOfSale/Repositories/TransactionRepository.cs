using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Models;

namespace MerlinPointOfSale.Repositories
{
    public class TransactionRepository
    {
        private readonly string connectionString;

        public TransactionRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void AddTransaction(Transaction transaction)
        {
            string sql = @"INSERT INTO Transactions (TransactionNumber, RegisterNumber, LocationID, TransactionDate, 
                            TransactionTime, EmployeeID, CustomerID, Subtotal, Taxes, TotalAmount, PaymentMethod, NetCash) 
                           VALUES (@TransactionNumber, @RegisterNumber, @LocationID, @TransactionDate, @TransactionTime, 
                            @EmployeeID, @CustomerID, @Subtotal, @Taxes, @TotalAmount, @PaymentMethod, @NetCash)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TransactionNumber", transaction.TransactionNumber);
                    cmd.Parameters.AddWithValue("@RegisterNumber", transaction.RegisterNumber);
                    cmd.Parameters.AddWithValue("@LocationID", transaction.LocationID);
                    cmd.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
                    cmd.Parameters.AddWithValue("@TransactionTime", transaction.TransactionTime);
                    cmd.Parameters.AddWithValue("@EmployeeID", transaction.EmployeeID);
                    cmd.Parameters.AddWithValue("@CustomerID", transaction.CustomerID);
                    cmd.Parameters.AddWithValue("@Subtotal", transaction.Subtotal);
                    cmd.Parameters.AddWithValue("@Taxes", transaction.Taxes);
                    cmd.Parameters.AddWithValue("@TotalAmount", transaction.TotalAmount);
                    cmd.Parameters.AddWithValue("@PaymentMethod", transaction.PaymentMethod);
                    cmd.Parameters.AddWithValue("@NetCash", transaction.NetCash);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
