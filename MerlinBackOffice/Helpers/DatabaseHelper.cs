using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MerlinBackOffice.Helpers
{
    public class DatabaseHelper
    {
        public string GetConnectionString()
        {
            string connectionString = Properties.Settings.Default.DatabaseConnection;
            return connectionString;
        }

        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            string connectionString = GetConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            connection.Open();
                            adapter.Fill(dataTable);
                            return dataTable;
                        }
                    }

                    catch (Exception exception)
                    {
                        ShowDatabaseError(exception);
                        throw;
                    }
                }
            }
        }

        public object ExecuteScalarQuery(string query, SqlParameter[] parameters = null)
        {
            string connectionString = GetConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        return command.ExecuteScalar();
                    }
                    catch (Exception exception)
                    {
                        ShowDatabaseError(exception);
                        return null; // Return null in case of failure
                    }
                }
            }
        }




        public void ShowDatabaseError(Exception exception)
        {
            MessageBox.Show("Database error: " + exception.Message);
        }
    }

}

