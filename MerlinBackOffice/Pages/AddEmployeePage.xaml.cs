using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinBackOffice.Models;
using MerlinBackOffice.Helpers;

namespace MerlinBackOffice.Pages
{
    public partial class AddEmployeePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private bool isTipEligible = false; // Default to false


        public AddEmployeePage()
        {
            InitializeComponent();
            PopulateStateComboBox();
            PopulateLocationComboBox();
        }

        private void SaveEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Collect field values
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string streetAddress = StreetAddressTextBox.Text.Trim();
            string city = CityTextBox.Text.Trim();
            string state = StateComboBox.Text;
            string zip = ZIPTextBox.Text.Trim();
            string phoneNumber = PhoneNumberTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string ssn = SSNTextBox.Text.Trim();
            string employeeType = EmployeeTypeComboBox.Text;
            string wageText = WageTextBox.Text.Trim();
            decimal wage = !string.IsNullOrEmpty(wageText) && decimal.TryParse(wageText, out decimal result) ? result : 0;

            string payRate = PayRateComboBox.Text;
            string payFrequency = PayFrequencyComboBox.Text;
            bool isOvertimeEligible = rbOtYes.IsChecked == true;
            bool isCommissionEligible = rbCoYes.IsChecked == true;
            bool isTipEligible = rbTipYes.IsChecked == true; // Read Tip Eligibility

            // Get commission rate and limit
            string commissionRateText = CommissionPercentageTextBox.Text.Trim();
            decimal commissionRate = isCommissionEligible && !string.IsNullOrEmpty(commissionRateText) && decimal.TryParse(commissionRateText, out decimal rateResult)
                ? rateResult
                : 0;

            string commissionLimitText = CommissionLimitTextBox.Text.Trim();
            decimal commissionLimit = isCommissionEligible && !string.IsNullOrEmpty(commissionLimitText) && decimal.TryParse(commissionLimitText, out decimal limitResult)
                ? limitResult
                : 0;

            // Calculate overtime wage if overtime eligible
            decimal overtimeWage = isOvertimeEligible ? wage * 1.5M : 0;

            // Get the selected location ID
            string primaryLocationID = (PrimaryLocationComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

            if (string.IsNullOrEmpty(primaryLocationID))
            {
                MessageBox.Show("Please select a primary location for the employee.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Generate EmployeeID and related fields
                string employeeID = GenerateEmployeeID();
                string initials = GenerateEmployeeInitials(firstName, lastName);
                string password = lastName.ToUpper(); // Placeholder password
                string transactionPIN = zip; // PIN derived from ZIP

                // Insert into database
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
            INSERT INTO Employees (
                EmployeeID, EmployeeFirstName, EmployeeLastName, 
                EmployeePhoneNumber, EmployeeEmail, EmployeeStreetAddress, EmployeeCity, 
                EmployeeState, EmployeeZIP, EmployeeSSN, EmployeePassword, EmployeeTransactionPIN, 
                EmployeeInitials, EmployeeType, EmployeeWage, EmployeePayRate, 
                EmployeePayFrequency, EmployeeOvertimeEligible, EmployeeOvertimeWage, 
                EmployeeCommissionEligible, EmployeeCommissionRate, EmployeeCommissionLimit,
                EmployeeTipEligible, EmployeePrimaryLocationID
            ) 
            VALUES (
                @EmployeeID, @FirstName, @LastName, @PhoneNumber, @Email, @StreetAddress, 
                @City, @State, @ZIP, @SSN, @Password, @TransactionPIN, @Initials, @EmployeeType, 
                @Wage, @PayRate, @PayFrequency, @OvertimeEligible, @OvertimeWage, 
                @CommissionEligible, @CommissionRate, @CommissionLimit, 
                @TipEligible, @PrimaryLocationID
            )";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@StreetAddress", streetAddress);
                        cmd.Parameters.AddWithValue("@City", city);
                        cmd.Parameters.AddWithValue("@State", state);
                        cmd.Parameters.AddWithValue("@ZIP", zip);
                        cmd.Parameters.AddWithValue("@SSN", ssn);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@TransactionPIN", transactionPIN);
                        cmd.Parameters.AddWithValue("@Initials", initials);
                        cmd.Parameters.AddWithValue("@EmployeeType", employeeType);
                        cmd.Parameters.AddWithValue("@Wage", wage);
                        cmd.Parameters.AddWithValue("@PayRate", payRate);
                        cmd.Parameters.AddWithValue("@PayFrequency", payFrequency);
                        cmd.Parameters.AddWithValue("@OvertimeEligible", isOvertimeEligible);
                        cmd.Parameters.AddWithValue("@OvertimeWage", overtimeWage);
                        cmd.Parameters.AddWithValue("@CommissionEligible", isCommissionEligible);
                        cmd.Parameters.AddWithValue("@CommissionRate", commissionRate);
                        cmd.Parameters.AddWithValue("@CommissionLimit", commissionLimit);
                        cmd.Parameters.AddWithValue("@TipEligible", isTipEligible); // Store Tip Eligibility
                        cmd.Parameters.AddWithValue("@PrimaryLocationID", primaryLocationID);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Employee added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string GenerateEmployeeID()
        {
            Random rand = new Random();
            return rand.Next(100000, 999999).ToString();
        }

        private string GenerateEmployeeInitials(string firstName, string lastName)
        {
            if (firstName.Length > 0 && lastName.Length > 1)
                return $"{firstName[0]}{lastName[0]}{lastName[1]}".ToUpper();
            return string.Empty;
        }

        private void WageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rbOtYes.IsChecked == true && decimal.TryParse(WageTextBox.Text.Trim(), out decimal wage))
            {
                decimal overtimeRate = wage * 1.5M;
                OvertimeWageTextBox.Text = overtimeRate.ToString("F2");
            }
        }

        private void rbOtYes_Checked(object sender, RoutedEventArgs e)
        {
            OvertimeWageSection.Visibility = Visibility.Visible;
            WageTextBox_TextChanged(null, null);
        }

        private void rbOtNo_Checked(object sender, RoutedEventArgs e)
        {
            OvertimeWageTextBox.Text = string.Empty;
            OvertimeWageSection.Visibility = Visibility.Collapsed;
        }

        private void rbCoYes_Checked(object sender, RoutedEventArgs e)
        {
            CommissionSection.Visibility = Visibility.Visible;
        }

        private void rbCoNo_Checked(object sender, RoutedEventArgs e)
        {
            CommissionSection.Visibility = Visibility.Collapsed;
            CommissionPercentageTextBox.Text = string.Empty; // Clear the text box
            CommissionLimitTextBox.Text = string.Empty; // Clear the text box
        }

        private void rbTipYes_Checked(object sender, RoutedEventArgs e)
        {
            isTipEligible = true; // Set Tip Eligible flag
        }

        private void rbTipNo_Checked(object sender, RoutedEventArgs e)
        {
            isTipEligible = false; // Unset Tip Eligible flag
        }


        private void PopulateStateComboBox()
        {
            // List of U.S. state abbreviations
            string[] stateAbbreviations = new string[]
            {
                "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
                "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
                "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
                "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
                "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"
            };

            // Add state abbreviations to the combo box
            foreach (var state in stateAbbreviations)
            {
                StateComboBox.Items.Add(state);
            }
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
                                Tag = locationID // Store the LocationID in the Tag property
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

    }
}
