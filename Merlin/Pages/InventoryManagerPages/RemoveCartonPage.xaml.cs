using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.InventoryManagerPages
{
    public partial class RemoveCartonPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper(); // Assuming a helper class is available

        public RemoveCartonPage()
        {
            InitializeComponent();
        }

        // Handle the search button click event
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string cartonID = CartonIDTextBox.Text.Trim();
            if (string.IsNullOrEmpty(cartonID))
            {
                MessageBox.Show("Please enter a valid Carton ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT * FROM Cartons WHERE CartonID = @CartonID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CartonID", cartonID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CartonDetailsSection.Visibility = Visibility.Visible;

                                OriginTextBlock.Text = reader["CartonOrigin"].ToString();
                                DestinationTextBlock.Text = reader["CartonDestination"].ToString();
                                StatusTextBlock.Text = reader["CartonStatus"].ToString();
                                ShipDateTextBlock.Text = Convert.ToDateTime(reader["CartonShipDate"]).ToShortDateString();
                                ReceiveDateTextBlock.Text = reader["CartonReceiveDate"] != DBNull.Value ? Convert.ToDateTime(reader["CartonReceiveDate"]).ToShortDateString() : "N/A";
                                ReceiveEmployeeTextBlock.Text = reader["CartonReceiveEmployee"]?.ToString() ?? "N/A";
                            }
                            else
                            {
                                MessageBox.Show("Carton not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handle the remove button click event
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            string cartonID = CartonIDTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(cartonID))
            {
                MessageBox.Show("Please enter a valid Carton ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete carton {cartonID}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                    {
                        conn.Open();
                        // Start a transaction to ensure atomicity of deletion from both tables
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            try
                            {
                                // First, delete the carton details from CartonDetails table
                                string deleteCartonDetailsQuery = "DELETE FROM CartonDetails WHERE CartonID = @CartonID";
                                using (SqlCommand deleteDetailsCmd = new SqlCommand(deleteCartonDetailsQuery, conn, transaction))
                                {
                                    deleteDetailsCmd.Parameters.AddWithValue("@CartonID", cartonID);
                                    deleteDetailsCmd.ExecuteNonQuery();
                                }

                                // Then, delete the carton from Cartons table
                                string deleteCartonQuery = "DELETE FROM Cartons WHERE CartonID = @CartonID";
                                using (SqlCommand deleteCartonCmd = new SqlCommand(deleteCartonQuery, conn, transaction))
                                {
                                    deleteCartonCmd.Parameters.AddWithValue("@CartonID", cartonID);
                                    deleteCartonCmd.ExecuteNonQuery();
                                }

                                // Commit the transaction if both deletions are successful
                                transaction.Commit();
                                MessageBox.Show($"Carton {cartonID} and its details were deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            }
                            catch (Exception ex)
                            {
                                // Rollback the transaction if there was an error
                                transaction.Rollback();
                                MessageBox.Show($"Error deleting carton: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
