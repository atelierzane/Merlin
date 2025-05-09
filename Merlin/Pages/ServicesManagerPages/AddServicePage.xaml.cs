using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.ServicesManagerPages
{
    public partial class AddServicePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private string currentServiceID;

        public AddServicePage()
        {
            InitializeComponent();
        }

        public AddServicePage(string serviceID) : this()
        {
            currentServiceID = serviceID;
            LoadServiceDetails(serviceID);
        }

        private void LoadServiceDetails(string serviceID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Load Service Details
                    string serviceQuery = "SELECT * FROM Services WHERE ServiceID = @ServiceID";
                    using (SqlCommand serviceCmd = new SqlCommand(serviceQuery, conn))
                    {
                        serviceCmd.Parameters.AddWithValue("@ServiceID", serviceID);
                        using (SqlDataReader reader = serviceCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ServiceNameTextBox.Text = reader["ServiceName"].ToString();
                                ServicePriceTextBox.Text = reader["ServicePrice"].ToString();

                                string pricedBy = reader["ServicePricedBy"].ToString();
                                foreach (ComboBoxItem item in ServicePricedByComboBox.Items)
                                {
                                    if (item.Content.ToString() == pricedBy)
                                    {
                                        ServicePricedByComboBox.SelectedItem = item;
                                        break;
                                    }
                                }

                                ServicePlusCheckBox.IsChecked = (bool)reader["ServicePlus"];
                                ServiceFeesCheckBox.IsChecked = (bool)reader["ServiceFees"];
                            }
                        }
                    }

                    // Load Add-Ons and Fees
                    LoadAddOns(serviceID, conn);
                    LoadFees(serviceID, conn);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadAddOns(string serviceID, SqlConnection conn)
        {
            if (ServicePlusCheckBox.IsChecked == true)
            {
                ServiceAddOnsList.Items.Clear();
                string addOnsQuery = "SELECT ServicePlusID, ServicePlusName, ServicePlusPrice, ServicePlusPricedBy FROM ServiceAddOns WHERE ServiceID = @ServiceID";
                using (SqlCommand addOnsCmd = new SqlCommand(addOnsQuery, conn))
                {
                    addOnsCmd.Parameters.AddWithValue("@ServiceID", serviceID);
                    using (SqlDataReader reader = addOnsCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string addOnDetails = $"{reader["ServicePlusName"]} - {reader["ServicePlusPrice"]:C} ({reader["ServicePlusPricedBy"]})";
                            ServiceAddOnsList.Items.Add(addOnDetails);
                        }
                    }
                }
            }
        }

        private void LoadFees(string serviceID, SqlConnection conn)
        {
            if (ServiceFeesCheckBox.IsChecked == true)
            {
                ServiceFeesList.Items.Clear();
                string feesQuery = "SELECT ServiceFeeID, ServiceFeeName, ServiceFeePrice, ServiceFeePricedBy FROM ServiceFees WHERE ServiceID = @ServiceID";
                using (SqlCommand feesCmd = new SqlCommand(feesQuery, conn))
                {
                    feesCmd.Parameters.AddWithValue("@ServiceID", serviceID);
                    using (SqlDataReader reader = feesCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string feeDetails = $"{reader["ServiceFeeName"]} - {reader["ServiceFeePrice"]:C} ({reader["ServiceFeePricedBy"]})";
                            ServiceFeesList.Items.Add(feeDetails);
                        }
                    }
                }
            }
        }

        private void SaveServiceButton_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = ServiceNameTextBox.Text.Trim();
            string servicePriceText = ServicePriceTextBox.Text.Trim();
            decimal servicePrice;
            string servicePricedBy = (ServicePricedByComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            bool servicePlus = ServicePlusCheckBox.IsChecked ?? false;
            bool serviceFees = ServiceFeesCheckBox.IsChecked ?? false;

            // Input validation
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                MessageBox.Show("Service Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(servicePriceText, out servicePrice) || servicePrice <= 0)
            {
                MessageBox.Show("Please enter a valid price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(servicePricedBy))
            {
                MessageBox.Show("Please select a pricing method.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string serviceID = currentServiceID ?? GenerateUniqueID("SRV");

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Use INSERT or UPDATE based on whether the service is new or being edited
                    string query = currentServiceID == null
                        ? "INSERT INTO Services (ServiceID, ServiceName, ServicePrice, ServicePricedBy, ServicePlus, ServiceFees) " +
                          "VALUES (@ServiceID, @ServiceName, @ServicePrice, @ServicePricedBy, @ServicePlus, @ServiceFees)"
                        : "UPDATE Services SET ServiceName = @ServiceName, ServicePrice = @ServicePrice, ServicePricedBy = @ServicePricedBy, " +
                          "ServicePlus = @ServicePlus, ServiceFees = @ServiceFees WHERE ServiceID = @ServiceID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ServiceID", serviceID);
                        cmd.Parameters.AddWithValue("@ServiceName", serviceName);
                        cmd.Parameters.AddWithValue("@ServicePrice", servicePrice);
                        cmd.Parameters.AddWithValue("@ServicePricedBy", servicePricedBy);
                        cmd.Parameters.AddWithValue("@ServicePlus", servicePlus);
                        cmd.Parameters.AddWithValue("@ServiceFees", serviceFees);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Service saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string GenerateUniqueID(string prefix)
        {
            Random random = new Random();
            return prefix + random.Next(100000, 999999).ToString();
        }

        private void ClearForm()
        {
            ServiceNameTextBox.Clear();
            ServicePriceTextBox.Clear();
            ServicePricedByComboBox.SelectedIndex = -1;
            ServicePlusCheckBox.IsChecked = false;
            ServiceFeesCheckBox.IsChecked = false;
            ServiceAddOnsList.Items.Clear();
            ServiceFeesList.Items.Clear();
            currentServiceID = null;
        }

        private void ServicePlusCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ServiceAddOnsPanel.Visibility = ServicePlusCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ServiceFeesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ServiceFeesPanel.Visibility = ServiceFeesCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
