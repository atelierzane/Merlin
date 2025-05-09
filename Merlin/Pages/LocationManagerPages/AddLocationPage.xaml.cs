using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.LocationManagerPages
{
    public partial class AddLocationPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private List<Employee> allManagers = new List<Employee>();


        public AddLocationPage()
        {
            InitializeComponent();
            LoadStateComboBox();
            LoadManagers();
            LoadDropdowns(); // Load the dropdown data
        }

        // Load managers into the ComboBox and store them for filtering
        private void LoadManagers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT EmployeeID, EmployeeFirstName, EmployeeLastName, EmployeeEmail, EmployeePhoneNumber, EmployeeType FROM Employees WHERE EmployeeType IN ('Manager', 'Administrator')";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var employee = new Employee
                                {
                                    EmployeeID = reader["EmployeeID"].ToString().Trim(),
                                    FirstName = reader["EmployeeFirstName"].ToString().Trim(),
                                    LastName = reader["EmployeeLastName"].ToString().Trim(),
                                    Email = reader["EmployeeEmail"].ToString().Trim(),
                                    PhoneNumber = reader["EmployeePhoneNumber"].ToString().Trim(),
                                    EmployeeType = reader["EmployeeType"].ToString().Trim()
                                };

                                allManagers.Add(employee);
                            }
                        }
                    }
                }

                ManagerComboBox.ItemsSource = allManagers;
                ManagerComboBox.DisplayMemberPath = "DisplayName";
                ManagerComboBox.SelectedValuePath = "EmployeeID";
                ManagerComboBox.IsEditable = false;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading managers: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Load Divisions, Markets, Regions, and Districts into their respective ComboBoxes
        private void LoadDropdowns()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    // Load Divisions
                    LoadComboBox("SELECT DivisionID, DivisionName FROM Divisions", DivisionComboBox, "DivisionID", "DivisionName");

                    // Load Markets
                    LoadComboBox("SELECT MarketID, MarketName FROM Markets", MarketComboBox, "MarketID", "MarketName");

                    // Load Regions
                    LoadComboBox("SELECT RegionID, RegionName FROM Regions", RegionComboBox, "RegionID", "RegionName");

                    // Load Districts
                    LoadComboBox("SELECT DistrictID, DistrictName FROM Districts", DistrictComboBox, "DistrictID", "DistrictName");
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading dropdowns: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadComboBox(string query, ComboBox comboBox, string valueMember, string displayMember)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = reader[displayMember].ToString(),
                            Tag = reader[valueMember].ToString()
                        };
                        comboBox.Items.Add(item);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading data for ComboBox: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStateComboBox()
        {
            var states = new List<string>
            {
                "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
                "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
                "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
                "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
                "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"
            };

            foreach (var state in states)
            {
                StateComboBox.Items.Add(new ComboBoxItem
                {
                    Content = state,
                    Tag = state
                });
            }
        }



        private void ManagerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string employeeID = ManagerComboBox.SelectedValue as string;

            if (ManagerComboBox.SelectedItem is Employee selectedEmployee)
            {
                ManagerComboBox.Text = selectedEmployee.DisplayName;
            }

        }


        private void SaveLocation_Click(object sender, RoutedEventArgs e)
        {
            string locationID = LocationIDTextBox.Text.Trim();
            string streetAddress = StreetAddressTextBox.Text.Trim();
            string city = CityTextBox.Text.Trim();
            string state = (StateComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string zip = ZIPTextBox.Text.Trim();
            string phoneNumber = PhoneNumberTextBox.Text.Trim();
            string locationType = (LocationTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string managerID = ManagerComboBox.SelectedValue?.ToString() ?? string.Empty;
            bool isTradeHold = rbYes.IsChecked == true;
            int tradeHoldDuration = int.TryParse(TradeHoldDurationTextBox.Text, out int duration) ? duration : 0;

            string divisionID = (DivisionComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? string.Empty;
            string marketID = (MarketComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? string.Empty;
            string regionID = (RegionComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? string.Empty;
            string districtID = (DistrictComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? string.Empty;

            if (locationID.Length != 4 || !int.TryParse(locationID, out _))
            {
                MessageBox.Show("Location ID must be a 4-digit number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = "INSERT INTO Location (LocationID, LocationStreetAddress, LocationCity, LocationState, LocationZIP, LocationPhoneNumber, LocationType, " +
                                   "LocationManagerID, LocationIsTradeHold, LocationTradeHoldDuration, LocationDivisionID, LocationMarketID, LocationRegionID, LocationDistrictID) " +
                                   "VALUES (@LocationID, @StreetAddress, @City, @State, @ZIP, @PhoneNumber, @Type, @ManagerID, @IsTradeHold, @TradeHoldDuration, @LocationDivisionID, @LocationMarketID, @LocationRegionID, @LocationDistrictID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@LocationID", locationID);
                        cmd.Parameters.AddWithValue("@StreetAddress", streetAddress);
                        cmd.Parameters.AddWithValue("@City", city);
                        cmd.Parameters.AddWithValue("@State", state);
                        cmd.Parameters.AddWithValue("@ZIP", zip);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@Type", locationType);
                        cmd.Parameters.AddWithValue("@ManagerID", managerID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsTradeHold", isTradeHold);
                        cmd.Parameters.AddWithValue("@TradeHoldDuration", tradeHoldDuration);
                        cmd.Parameters.AddWithValue("@LocationDivisionID", divisionID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LocationMarketID", marketID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LocationRegionID", regionID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LocationDistrictID", districtID ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Location added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
