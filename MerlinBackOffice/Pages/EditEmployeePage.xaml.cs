using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinBackOffice.Helpers;

namespace MerlinBackOffice.Pages
{
    public partial class EditEmployeePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private bool isTipEligible;
        public EditEmployeePage()
        {
            InitializeComponent();
            PopulateLocationComboBox();
        }

        private void PopulateLocationComboBox()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT LocationID, LocationCity, LocationState FROM Location";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string locationID = reader["LocationID"].ToString();
                            string locationCity = reader["LocationCity"].ToString();
                            string locationState = reader["LocationState"].ToString();
                            string displayText = $"{locationCity}, {locationState} ({locationID})";

                            PrimaryLocationComboBox.Items.Add(new ComboBoxItem
                            {
                                Content = displayText,
                                Tag = locationID
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error loading locations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchEmployee_Click(object sender, RoutedEventArgs e)
        {
            string employeeID = SearchEmployeeTextBox.Text.Trim();

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT * FROM Employees WHERE EmployeeID = @EmployeeID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                FirstNameTextBox.Text = reader["EmployeeFirstName"].ToString();
                                LastNameTextBox.Text = reader["EmployeeLastName"].ToString();
                                WageTextBox.Text = reader["EmployeeWage"].ToString();
                                CommissionRateTextBox.Text = reader["EmployeeCommissionRate"].ToString();
                                CommissionLimitTextBox.Text = reader["EmployeeCommissionLimit"].ToString();

                                // Load Tip Eligibility
                                isTipEligible = Convert.ToBoolean(reader["EmployeeTipEligible"]);
                                rbTipYes.IsChecked = isTipEligible;
                                rbTipNo.IsChecked = !isTipEligible;

                                // Select primary location
                                string primaryLocationID = reader["EmployeePrimaryLocationID"].ToString();
                                foreach (ComboBoxItem item in PrimaryLocationComboBox.Items)
                                {
                                    if (item.Tag.ToString() == primaryLocationID)
                                    {
                                        PrimaryLocationComboBox.SelectedItem = item;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Employee not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error retrieving employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            string employeeID = SearchEmployeeTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string wageText = WageTextBox.Text.Trim();
            decimal wage = decimal.TryParse(wageText, out decimal result) ? result : 0;
            string commissionRateText = CommissionRateTextBox.Text.Trim();
            decimal commissionRate = decimal.TryParse(commissionRateText, out decimal rateResult) ? rateResult : 0;
            string commissionLimitText = CommissionLimitTextBox.Text.Trim();
            decimal commissionLimit = decimal.TryParse(commissionLimitText, out decimal limitResult) ? limitResult : 0;

            ComboBoxItem selectedLocation = PrimaryLocationComboBox.SelectedItem as ComboBoxItem;
            string primaryLocationID = selectedLocation?.Tag.ToString();

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"UPDATE Employees 
                             SET EmployeeFirstName = @FirstName, 
                                 EmployeeLastName = @LastName, 
                                 EmployeeWage = @Wage, 
                                 EmployeeCommissionRate = @CommissionRate, 
                                 EmployeeCommissionLimit = @CommissionLimit,
                                 EmployeePrimaryLocationID = @PrimaryLocationID,
                                 EmployeeTipEligible = @TipEligible
                             WHERE EmployeeID = @EmployeeID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@Wage", wage);
                        cmd.Parameters.AddWithValue("@CommissionRate", commissionRate);
                        cmd.Parameters.AddWithValue("@CommissionLimit", commissionLimit);
                        cmd.Parameters.AddWithValue("@PrimaryLocationID", primaryLocationID);
                        cmd.Parameters.AddWithValue("@TipEligible", isTipEligible);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Employee updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error updating employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void rbTipYes_Checked(object sender, RoutedEventArgs e)
        {
            isTipEligible = true;
        }

        private void rbTipNo_Checked(object sender, RoutedEventArgs e)
        {
            isTipEligible = false;
        }

    }
}
