using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.EmployeeManagerPages
{
    public partial class AddEmployeePage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

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
            string state = (StateComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string zip = ZIPTextBox.Text.Trim();
            string phoneNumber = PhoneNumberTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string ssn = SSNTextBox.Text.Trim();
            string employeeType = (EmployeeTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string primaryLocation = (PrimaryLocationComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? string.Empty;
            string wageText = WageTextBox.Text.Trim();
            decimal wage = !string.IsNullOrEmpty(wageText) && decimal.TryParse(wageText, out decimal result) ? result : 0;

            // Fetch Pay Rate and Pay Frequency
            string payRate = (PayRateComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            string payFrequency = (PayFrequencyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;

            bool isOvertimeEligible = rbOtYes.IsChecked == true;
            bool isCommissionEligible = rbCoYes.IsChecked == true;

            // Prompt for tip eligibility (add RadioButtons in XAML if not present)
            bool isTipEligible = rbTipYes?.IsChecked == true;

            // Calculate overtime wage if overtime eligible
            decimal overtimeWage = isOvertimeEligible ? wage * 1.5M : 0;

            // Prompt for commission rate and limit if eligible (add TextBoxes in XAML if not present)
            string commissionRateText = CommissionPercentageTextBox?.Text.Trim();
            decimal commissionRate = isCommissionEligible && !string.IsNullOrEmpty(commissionRateText) && decimal.TryParse(commissionRateText, out decimal rateResult) ? rateResult : 0;

            string commissionLimitText = CommissionLimitTextBox?.Text.Trim();
            decimal commissionLimit = isCommissionEligible && !string.IsNullOrEmpty(commissionLimitText) && decimal.TryParse(commissionLimitText, out decimal limitResult) ? limitResult : 0;

            try
            {
                // Generate EmployeeID, Initials, Password, and Transaction PIN
                string employeeID = GenerateEmployeeID();
                string initials = GenerateEmployeeInitials(firstName, lastName);
                string password = lastName.ToUpper(); // Using last name as placeholder password
                string transactionPIN = zip; // PIN derived from ZIP

                // Insert into database
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                INSERT INTO Employees (
                    EmployeeID, EmployeePrimaryLocationID, EmployeeFirstName, EmployeeLastName, 
                    EmployeePhoneNumber, EmployeeEmail, EmployeeStreetAddress, EmployeeCity, 
                    EmployeeState, EmployeeZIP, EmployeeSSN, EmployeePassword, 
                    EmployeeTransactionPIN, EmployeeInitials, EmployeeType, EmployeeWage, 
                    EmployeePayRate, EmployeePayFrequency, EmployeeOvertimeEligible, 
                    EmployeeOvertimeWage, EmployeeCommissionEligible, EmployeeCommissionRate, 
                    EmployeeCommissionLimit, EmployeeTipEligible
                ) 
                VALUES (
                    @EmployeeID, @PrimaryLocationID, @FirstName, @LastName, 
                    @PhoneNumber, @Email, @StreetAddress, @City, @State, @ZIP, 
                    @SSN, @Password, @TransactionPIN, @Initials, @EmployeeType, 
                    @Wage, @PayRate, @PayFrequency, @OvertimeEligible, @OvertimeWage, 
                    @CommissionEligible, @CommissionRate, @CommissionLimit, @TipEligible
                )";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                        cmd.Parameters.AddWithValue("@PrimaryLocationID", primaryLocation);
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
                        cmd.Parameters.AddWithValue("@TipEligible", isTipEligible);

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

        // Populate States
        private void PopulateStateComboBox()
        {
            string[] states = {
                "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
                "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
                "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
                "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
                "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"
            };

            foreach (var state in states)
                StateComboBox.Items.Add(new ComboBoxItem { Content = state });
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
                            string locationID = reader["LocationID"].ToString().Trim();
                            string locationCity = reader["LocationCity"].ToString().Trim();
                            string locationState = reader["LocationState"].ToString().Trim();
                            string displayText = $"{locationCity}, {locationState} ({locationID})";

                            PrimaryLocationComboBox.Items.Add(new ComboBoxItem
                            {
                                Content = displayText,
                                Tag = locationID // Store LocationID for later use
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


        private void rbCoYes_Checked(object sender, RoutedEventArgs e)
        {
            CommissionSection.Visibility = Visibility.Visible;
        }

        private void rbCoNo_Checked(object sender, RoutedEventArgs e)
        {
            CommissionSection.Visibility = Visibility.Collapsed;
            CommissionPercentageTextBox.Text = string.Empty; // Clear text when not applicable
            CommissionLimitTextBox.Text = string.Empty;
        }


        // Method to generate a random EmployeeID
        private string GenerateEmployeeID()
        {
            Random rand = new Random();
            return rand.Next(100000, 999999).ToString();
        }

        // Method to generate Employee Initials
        private string GenerateEmployeeInitials(string firstName, string lastName)
        {
            if (firstName.Length > 0 && lastName.Length > 1)
            {
                return $"{firstName[0]}{lastName[0]}{lastName[1]}".ToUpper();
            }
            return string.Empty;
        }

        // Event handler for wage textbox change to recalculate overtime wage
        private void WageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rbOtYes.IsChecked == true && decimal.TryParse(WageTextBox.Text.Trim(), out decimal wage))
            {
                decimal overtimeRate = wage * 1.5M; // Overtime is 1.5x the regular wage
                OvertimeWageTextBox.Text = overtimeRate.ToString("F2"); // Format the overtime rate to 2 decimal places
            }
        }

        // Event handler for overtime eligibility selection
        private void rbOtYes_Checked(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(WageTextBox.Text.Trim(), out decimal wage))
            {
                decimal overtimeRate = wage * 1.5M;
                OvertimeWageTextBox.Text = overtimeRate.ToString("F2"); // Format the overtime rate to 2 decimal places
            }
            OvertimeWageSection.Visibility = Visibility.Visible;
        }

        private void rbOtNo_Checked(object sender, RoutedEventArgs e)
        {
            OvertimeWageTextBox.Text = string.Empty;
            OvertimeWageSection.Visibility = Visibility.Collapsed;
        }
    }
}
