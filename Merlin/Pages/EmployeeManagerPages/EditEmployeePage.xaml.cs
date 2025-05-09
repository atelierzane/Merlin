using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.EmployeeManagerPages
{
    public partial class EditEmployeePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public EditEmployeePage()
        {
            InitializeComponent();
            PopulateComboBoxes();
        }

        // Method to populate ComboBoxes with necessary values
        private void PopulateComboBoxes()
        {
            EmployeeTypeComboBox.Items.Add(new ComboBoxItem { Content = "Sales" });
            EmployeeTypeComboBox.Items.Add(new ComboBoxItem { Content = "Manager" });
            EmployeeTypeComboBox.Items.Add(new ComboBoxItem { Content = "Administrator" });

            PayRateComboBox.Items.Add(new ComboBoxItem { Content = "Hourly" });
            PayRateComboBox.Items.Add(new ComboBoxItem { Content = "Annually" });

            PayFrequencyComboBox.Items.Add(new ComboBoxItem { Content = "Weekly" });
            PayFrequencyComboBox.Items.Add(new ComboBoxItem { Content = "Bi-weekly" });
            PayFrequencyComboBox.Items.Add(new ComboBoxItem { Content = "Monthly" });

            // Populate state ComboBox (assuming you want US states, but this can be adjusted)
            StateTextBox.Items.Add(new ComboBoxItem { Content = "CA" });
            StateTextBox.Items.Add(new ComboBoxItem { Content = "NY" });
            // Add other states as needed
        }

        // Search for the employee by Employee ID
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string employeeID = EmployeeIDTextBox.Text.Trim();

            if (string.IsNullOrEmpty(employeeID))
            {
                MessageBox.Show("Please enter an Employee ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                            if (reader.HasRows)
                            {
                                reader.Read();
                                // Populate the fields with employee data
                                FirstNameTextBox.Text = reader["EmployeeFirstName"].ToString();
                                LastNameTextBox.Text = reader["EmployeeLastName"].ToString();
                                EmailTextBox.Text = reader["EmployeeEmail"].ToString();
                                PhoneNumberTextBox.Text = reader["EmployeePhoneNumber"].ToString();
                                StreetAddressTextBox.Text = reader["EmployeeStreetAddress"].ToString();
                                CityTextBox.Text = reader["EmployeeCity"].ToString();
                                StateTextBox.SelectedItem = StateTextBox.Items
                                    .Cast<ComboBoxItem>()
                                    .FirstOrDefault(item => item.Content.ToString() == reader["EmployeeState"].ToString());

                                ZIPTextBox.Text = reader["EmployeeZIP"].ToString();
                                SSNTextBox.Text = reader["EmployeeSSN"].ToString();
                                WageTextBox.Text = reader["EmployeeWage"].ToString();

                                // Set PayRate and PayFrequency combo boxes
                                PayRateComboBox.SelectedItem = PayRateComboBox.Items
                                    .Cast<ComboBoxItem>()
                                    .FirstOrDefault(item => item.Content.ToString() == reader["EmployeePayRate"].ToString());

                                PayFrequencyComboBox.SelectedItem = PayFrequencyComboBox.Items
                                    .Cast<ComboBoxItem>()
                                    .FirstOrDefault(item => item.Content.ToString() == reader["EmployeePayFrequency"].ToString());

                                // Set employee type combo box
                                EmployeeTypeComboBox.SelectedItem = EmployeeTypeComboBox.Items
                                    .Cast<ComboBoxItem>()
                                    .FirstOrDefault(item => item.Content.ToString() == reader["EmployeeType"].ToString());

                                // Set overtime eligibility
                                rbOtYes.IsChecked = (bool)reader["EmployeeOvertimeEligible"];
                                rbOtNo.IsChecked = !(bool)reader["EmployeeOvertimeEligible"];

                                // Set commission eligibility
                                rbCoYes.IsChecked = (bool)reader["EmployeeCommissionEligible"];
                                rbCoNo.IsChecked = !(bool)reader["EmployeeCommissionEligible"];

                                // Show the edit section
                                EmployeeEditSection.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("No employee found with the given Employee ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                EmployeeEditSection.Visibility = Visibility.Collapsed;
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

        // Update the employee information
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string employeeID = EmployeeIDTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string phoneNumber = PhoneNumberTextBox.Text.Trim();
            string address = StreetAddressTextBox.Text.Trim();
            string city = CityTextBox.Text.Trim();
            string state = (StateTextBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string zip = ZIPTextBox.Text.Trim();
            string ssn = SSNTextBox.Text.Trim();
            decimal wage;

            if (!decimal.TryParse(WageTextBox.Text.Trim(), out wage))
            {
                MessageBox.Show("Please enter a valid wage amount.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string updateQuery = "UPDATE Employees SET EmployeeFirstName = @FirstName, EmployeeLastName = @LastName, EmployeeEmail = @Email, " +
                                         "EmployeePhoneNumber = @PhoneNumber, EmployeeStreetAddress = @StreetAddress, EmployeeCity = @City, " +
                                         "EmployeeState = @State, EmployeeZIP = @ZIP, EmployeeSSN = @SSN, EmployeeWage = @Wage, EmployeePayRate = @PayRate, " +
                                         "EmployeePayFrequency = @PayFrequency, EmployeeOvertimeEligible = @OvertimeEligible, EmployeeCommissionEligible = @CommissionEligible " +
                                         "WHERE EmployeeID = @EmployeeID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@StreetAddress", address);
                        cmd.Parameters.AddWithValue("@City", city);
                        cmd.Parameters.AddWithValue("@State", state);
                        cmd.Parameters.AddWithValue("@ZIP", zip);
                        cmd.Parameters.AddWithValue("@SSN", ssn);
                        cmd.Parameters.AddWithValue("@Wage", wage);
                        cmd.Parameters.AddWithValue("@PayRate", (PayRateComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                        cmd.Parameters.AddWithValue("@PayFrequency", (PayFrequencyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString());
                        cmd.Parameters.AddWithValue("@OvertimeEligible", rbOtYes.IsChecked == true);
                        cmd.Parameters.AddWithValue("@CommissionEligible", rbCoYes.IsChecked == true);
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Employee updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update the employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
