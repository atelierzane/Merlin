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
using MerlinBackOffice.Helpers;

namespace MerlinBackOffice.Windows.TradeHoldWindows
{
    public partial class TradeHoldWindow : Window
    {
        private DatabaseHelper databaseHelper = new DatabaseHelper();

        public TradeHoldWindow()
        {
            InitializeComponent();
            LoadTradeHoldCartons();
        }

        private void LoadTradeHoldCartons()
        {
            try
            {
                List<TradeHoldCarton> tradeHoldCartons = new List<TradeHoldCarton>();

                string query = @"
            SELECT 
                TradeHoldCartonID, 
                TradeHoldCartonExpirationDate, 
                TradeHoldCartonExpectedQuantity, 
                TradeHoldCartonIsFinalized 
            FROM LocationTradeHold 
            WHERE LocationID = @LocationID";

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
                                tradeHoldCartons.Add(new TradeHoldCarton
                                {
                                    CartonID = reader["TradeHoldCartonID"].ToString(),
                                    ExpirationDate = Convert.ToDateTime(reader["TradeHoldCartonExpirationDate"]),
                                    TotalQuantity = Convert.ToInt32(reader["TradeHoldCartonExpectedQuantity"]), // Use TradeHoldCartonExpectedQuantity
                                    IsFinalized = Convert.ToBoolean(reader["TradeHoldCartonIsFinalized"])
                                });
                            }
                        }
                    }
                }

                lvTradeHoldCartons.ItemsSource = tradeHoldCartons;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trade hold cartons: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnEditCarton_Click(object sender, RoutedEventArgs e)
        {
            if (lvTradeHoldCartons.SelectedItem is TradeHoldCarton selectedCarton)
            {
                // Check if the carton expiration date has passed
                if (DateTime.Now >= selectedCarton.ExpirationDate)
                {
                    // Open the TradeHoldCartonDetailsWindow in edit mode
                    TradeHoldCartonDetailsWindow detailsWindow = new TradeHoldCartonDetailsWindow(selectedCarton.CartonID, isEditable: true);
                    detailsWindow.ShowDialog();
                    LoadTradeHoldCartons();
                }
                else
                {
                    MessageBox.Show("This carton cannot be edited until after its expiration date.", "Edit Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a carton to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnCartonDetails_Click(object sender, RoutedEventArgs e)
        {
            if (lvTradeHoldCartons.SelectedItem is TradeHoldCarton selectedCarton)
            {
                // Open the TradeHoldCartonDetailsWindow in view-only mode
                TradeHoldCartonDetailsWindow detailsWindow = new TradeHoldCartonDetailsWindow(selectedCarton.CartonID, isEditable: false);
                detailsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a carton to view.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void OnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }

    public class TradeHoldCarton
    {
        public string CartonID { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int TotalQuantity { get; set; }
        public bool IsFinalized { get; set; }
    }
}
