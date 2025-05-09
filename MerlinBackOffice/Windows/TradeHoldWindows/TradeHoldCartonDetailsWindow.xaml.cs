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
using MerlinBackOffice.Properties;

namespace MerlinBackOffice.Windows.TradeHoldWindows
{
    public partial class TradeHoldCartonDetailsWindow : Window
    {
        private DatabaseHelper databaseHelper = new DatabaseHelper();
        private readonly string cartonID;
        private readonly bool isEditable;
        private string locationID;

        public TradeHoldCartonDetailsWindow(string cartonID, bool isEditable)
        {
            locationID = Properties.Settings.Default.LocationID;
            InitializeComponent();
            this.cartonID = cartonID;
            this.isEditable = isEditable;
            LoadCartonDetails(cartonID, locationID);
            SetEditMode(isEditable);
        }

        private void LoadCartonDetails(string cartonID, string locationID)
        {
            try
            {
                List<TradeHoldCartonDetail> tradeHoldCartonDetails = new List<TradeHoldCartonDetail>();

                string query = @"
                SELECT 
                    d.SKU,
                    c.ProductName,
                    d.CartonExpectedQuantitySKUSellable AS SellableQuantity,
                    d.CartonExpectedQuantitySKUDefective AS DefectiveQuantity,
                    d.CartonActualQuantitySKUSellable AS ConfirmedSellableQuantity,
                    d.CartonActualQuantitySKUDefective AS ConfirmedDefectiveQuantity
                FROM LocationTradeHoldCartonDetails d
                LEFT JOIN Catalog c ON d.SKU = c.SKU
                WHERE d.LocationID = @LocationID AND d.LocationTradeHoldCartonID = @CartonID";

                using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LocationID", locationID);
                        command.Parameters.AddWithValue("@CartonID", cartonID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tradeHoldCartonDetails.Add(new TradeHoldCartonDetail
                                {
                                    SKU = reader["SKU"].ToString(),
                                    ProductName = reader["ProductName"] != DBNull.Value ? reader["ProductName"].ToString() : "Unknown Product",
                                    SellableQuantity = Convert.ToInt32(reader["SellableQuantity"]),
                                    DefectiveQuantity = Convert.ToInt32(reader["DefectiveQuantity"]),
                                    ConfirmedSellableQuantity = Convert.ToInt32(reader["ConfirmedSellableQuantity"]),
                                    ConfirmedDefectiveQuantity = Convert.ToInt32(reader["ConfirmedDefectiveQuantity"])
                                });
                            }
                        }
                    }
                }

                lvTradeHoldDetails.ItemsSource = tradeHoldCartonDetails;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trade hold carton details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditable)
            {
                MessageBox.Show("This carton cannot be edited or finalized because it is in view-only mode.", "Not Editable", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var cartonDetails = lvTradeHoldDetails.ItemsSource as List<TradeHoldCartonDetail>;

                if (cartonDetails != null)
                {
                    using (SqlConnection connection = new SqlConnection(databaseHelper.GetConnectionString()))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            foreach (var detail in cartonDetails)
                            {
                                string updateQuery = @"
                                UPDATE LocationTradeHoldCartonDetails 
                                SET CartonActualQuantitySKUSellable = @SellableQuantity, 
                                    CartonActualQuantitySKUDefective = @DefectiveQuantity 
                                WHERE LocationTradeHoldCartonID = @CartonID AND SKU = @SKU";

                                using (SqlCommand command = new SqlCommand(updateQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@SellableQuantity", detail.ConfirmedSellableQuantity);
                                    command.Parameters.AddWithValue("@DefectiveQuantity", detail.ConfirmedDefectiveQuantity);
                                    command.Parameters.AddWithValue("@CartonID", cartonID);
                                    command.Parameters.AddWithValue("@SKU", detail.SKU);

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                    }

                    MessageBox.Show("Carton details confirmed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error confirming carton details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetEditMode(bool isEditable)
        {
            // Disable or enable editing on the text boxes
            foreach (var item in lvTradeHoldDetails.Items)
            {
                var container = lvTradeHoldDetails.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container != null)
                {
                    TextBox sellableTextBox = container.FindName("ConfirmSellableQuantity") as TextBox;
                    if (sellableTextBox != null)
                    {
                        sellableTextBox.IsReadOnly = !isEditable;
                    }

                    TextBox defectiveTextBox = container.FindName("ConfirmDefectiveQuantity") as TextBox;
                    if (defectiveTextBox != null)
                    {
                        defectiveTextBox.IsReadOnly = !isEditable;
                    }
                }
            }

            // Enable or disable the "Confirm" button
            Button confirmButton = LogicalTreeHelper.FindLogicalNode(this, "OnConfirm_Click") as Button;
            if (confirmButton != null)
            {
                confirmButton.IsEnabled = isEditable;
            }
        }

        private void OnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class TradeHoldCartonDetail
    {
        public string SKU { get; set; }
        public string ProductName { get; set; } // Product Name
        public int SellableQuantity { get; set; }
        public int DefectiveQuantity { get; set; }
        public int ConfirmedSellableQuantity { get; set; }
        public int ConfirmedDefectiveQuantity { get; set; }
    }
}
