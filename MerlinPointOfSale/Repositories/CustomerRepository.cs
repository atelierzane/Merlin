using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MerlinPointOfSale.Models;

namespace MerlinPointOfSale.Repositories
{
    public class CustomerRepository
    {
        private readonly string connectionString;

        public CustomerRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public Customer GetCustomerByID(string customerID)
        {
            Customer customer = null;
            string sql = "SELECT * FROM Customers WHERE CustomerID = @customerID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@customerID", customerID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new Customer
                            {
                                CustomerID = reader["CustomerID"].ToString(),
                                CustomerFirstName = reader["CustomerFirstName"].ToString(),
                                CustomerLastName = reader["CustomerLastName"].ToString(),
                                CustomerPhoneNumber = reader["CustomerPhoneNumber"].ToString(),
                                CustomerEmail = reader["CustomerEmail"].ToString(),
                                CustomerStoreCredit = Convert.ToDecimal(reader["CustomerStoreCredit"]),
                                CustomerPoints = Convert.ToInt32(reader["CustomerPoints"])
                            };
                        }
                    }
                }
            }

            return customer;
        }

        public void UpdateCustomerLoyaltyPoints(string customerID, int points)
        {
            string sql = "UPDATE Customers SET CustomerPoints = CustomerPoints + @points WHERE CustomerID = @customerID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@customerID", customerID);
                    cmd.Parameters.AddWithValue("@points", points);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
