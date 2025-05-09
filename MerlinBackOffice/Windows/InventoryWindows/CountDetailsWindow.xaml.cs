using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using MerlinBackOffice.Helpers;
using MerlinBackOffice.Models;

namespace MerlinBackOffice.Windows.InventoryWindows
{
    public partial class CountDetailsWindow : Window
    {
        private ObservableCollection<CountDetail> countDetails;
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();
        private string countID;

        public CountDetailsWindow(string countID)
        {
            InitializeComponent();
            this.countID = countID;
            LoadCountDetails();
        }

        private void LoadCountDetails()
        {
            countDetails = new ObservableCollection<CountDetail>();

            string query = @"
            SELECT 
                SKU, 
                SKUQuantityExpected, 
                SKUQuantityActual, 
                SKUDiscrepancy
            FROM CountDetails
            WHERE CountID = @CountID";

            try
            {
                using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CountID", countID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                countDetails.Add(new CountDetail
                                {
                                    SKU = reader["SKU"].ToString(),
                                    SKUQuantityExpected = Convert.ToInt32(reader["SKUQuantityExpected"]),
                                    SKUQuantityActual = Convert.ToInt32(reader["SKUQuantityActual"]),
                                    SKUDiscrepancy = Convert.ToInt32(reader["SKUDiscrepancy"])
                                });
                            }
                        }
                    }
                }

                lvCountDetails.ItemsSource = countDetails;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading count details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
