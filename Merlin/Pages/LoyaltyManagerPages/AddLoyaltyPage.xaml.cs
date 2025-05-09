using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.LoyaltyManagerPages
{
    public partial class AddLoyaltyPage : Page
    {
        private readonly DatabaseHelper databaseHelper = new DatabaseHelper();
        private int visibleTiers = 0;

        public AddLoyaltyPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLoyaltyProgram();
        }

        private void LoadLoyaltyProgram()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                SELECT LoyaltyProgramName,
                       LoyaltyTier1Name, LoyaltyTier1PointsPerDollar, LoyaltyTier1IsPaid, LoyaltyTier1MonthlyPrice, LoyaltyTier1AnnualPrice, LoyaltyTier1TermMonthly, LoyaltyTier1TermAnnual,
                       LoyaltyTier2Name, LoyaltyTier2PointsPerDollar, LoyaltyTier2IsPaid, LoyaltyTier2MonthlyPrice, LoyaltyTier2AnnualPrice, LoyaltyTier2TermMonthly, LoyaltyTier2TermAnnual,
                       LoyaltyTier3Name, LoyaltyTier3PointsPerDollar, LoyaltyTier3IsPaid, LoyaltyTier3MonthlyPrice, LoyaltyTier3AnnualPrice, LoyaltyTier3TermMonthly, LoyaltyTier3TermAnnual
                FROM Loyalty";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                LoyaltyProgramNameTextBox.Text = reader["LoyaltyProgramName"].ToString();

                                Tier1NameTextBox.Text = reader["LoyaltyTier1Name"]?.ToString() ?? string.Empty;
                                Tier1PointsPerDollarTextBox.Text = reader["LoyaltyTier1PointsPerDollar"]?.ToString() ?? "0";
                                Tier1IsPaidCheckBox.IsChecked = reader["LoyaltyTier1IsPaid"] as bool? ?? false;

                                Tier2NameTextBox.Text = reader["LoyaltyTier2Name"]?.ToString() ?? string.Empty;
                                Tier2PointsPerDollarTextBox.Text = reader["LoyaltyTier2PointsPerDollar"]?.ToString() ?? "0";
                                Tier2IsPaidCheckBox.IsChecked = reader["LoyaltyTier2IsPaid"] as bool? ?? false;

                                Tier3NameTextBox.Text = reader["LoyaltyTier3Name"]?.ToString() ?? string.Empty;
                                Tier3PointsPerDollarTextBox.Text = reader["LoyaltyTier3PointsPerDollar"]?.ToString() ?? "0";
                                Tier3IsPaidCheckBox.IsChecked = reader["LoyaltyTier3IsPaid"] as bool? ?? false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading loyalty program: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveLoyaltyProgram_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(databaseHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = @"
                IF EXISTS (SELECT 1 FROM Loyalty WHERE LoyaltyProgramName = @ProgramName)
                BEGIN
                    UPDATE Loyalty
                    SET 
                        LoyaltyTier1Name = @Tier1Name, LoyaltyTier1PointsPerDollar = @Tier1Points, LoyaltyTier1IsPaid = @Tier1IsPaid, 
                        LoyaltyTier1MonthlyPrice = @Tier1MonthlyPrice, LoyaltyTier1AnnualPrice = @Tier1AnnualPrice,
                        LoyaltyTier1TermMonthly = @Tier1TermMonthly, LoyaltyTier1TermAnnual = @Tier1TermAnnual,
                        
                        LoyaltyTier2Name = @Tier2Name, LoyaltyTier2PointsPerDollar = @Tier2Points, LoyaltyTier2IsPaid = @Tier2IsPaid, 
                        LoyaltyTier2MonthlyPrice = @Tier2MonthlyPrice, LoyaltyTier2AnnualPrice = @Tier2AnnualPrice,
                        LoyaltyTier2TermMonthly = @Tier2TermMonthly, LoyaltyTier2TermAnnual = @Tier2TermAnnual,
                        
                        LoyaltyTier3Name = @Tier3Name, LoyaltyTier3PointsPerDollar = @Tier3Points, LoyaltyTier3IsPaid = @Tier3IsPaid, 
                        LoyaltyTier3MonthlyPrice = @Tier3MonthlyPrice, LoyaltyTier3AnnualPrice = @Tier3AnnualPrice,
                        LoyaltyTier3TermMonthly = @Tier3TermMonthly, LoyaltyTier3TermAnnual = @Tier3TermAnnual
                    WHERE LoyaltyProgramName = @ProgramName
                END
                ELSE
                BEGIN
                    INSERT INTO Loyalty 
                        (LoyaltyProgramName, 
                        LoyaltyTier1Name, LoyaltyTier1PointsPerDollar, LoyaltyTier1IsPaid, LoyaltyTier1MonthlyPrice, LoyaltyTier1AnnualPrice, LoyaltyTier1TermMonthly, LoyaltyTier1TermAnnual,
                        LoyaltyTier2Name, LoyaltyTier2PointsPerDollar, LoyaltyTier2IsPaid, LoyaltyTier2MonthlyPrice, LoyaltyTier2AnnualPrice, LoyaltyTier2TermMonthly, LoyaltyTier2TermAnnual,
                        LoyaltyTier3Name, LoyaltyTier3PointsPerDollar, LoyaltyTier3IsPaid, LoyaltyTier3MonthlyPrice, LoyaltyTier3AnnualPrice, LoyaltyTier3TermMonthly, LoyaltyTier3TermAnnual)
                    VALUES 
                        (@ProgramName, 
                        @Tier1Name, @Tier1Points, @Tier1IsPaid, @Tier1MonthlyPrice, @Tier1AnnualPrice, @Tier1TermMonthly, @Tier1TermAnnual,
                        @Tier2Name, @Tier2Points, @Tier2IsPaid, @Tier2MonthlyPrice, @Tier2AnnualPrice, @Tier2TermMonthly, @Tier2TermAnnual,
                        @Tier3Name, @Tier3Points, @Tier3IsPaid, @Tier3MonthlyPrice, @Tier3AnnualPrice, @Tier3TermMonthly, @Tier3TermAnnual)
                END";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProgramName", LoyaltyProgramNameTextBox.Text.Trim());

                        // Tier 1 Parameters
                        cmd.Parameters.AddWithValue("@Tier1Name", Tier1NameTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tier1Points", int.TryParse(Tier1PointsPerDollarTextBox.Text, out var tier1Points) ? tier1Points : 0);
                        cmd.Parameters.AddWithValue("@Tier1IsPaid", Tier1IsPaidCheckBox.IsChecked ?? false);
                        cmd.Parameters.AddWithValue("@Tier1MonthlyPrice", GetDecimal(Tier1MonthlyPriceTextBox.Text));
                        cmd.Parameters.AddWithValue("@Tier1AnnualPrice", GetDecimal(Tier1AnnualPriceTextBox.Text));
                        cmd.Parameters.AddWithValue("@Tier1TermMonthly", Tier1TermMonthlyCheckBox.IsChecked ?? false);
                        cmd.Parameters.AddWithValue("@Tier1TermAnnual", Tier1TermAnnualCheckBox.IsChecked ?? false);

                        // Tier 2 Parameters
                        cmd.Parameters.AddWithValue("@Tier2Name", Tier2NameTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tier2Points", int.TryParse(Tier2PointsPerDollarTextBox.Text, out var tier2Points) ? tier2Points : 0);
                        cmd.Parameters.AddWithValue("@Tier2IsPaid", Tier2IsPaidCheckBox.IsChecked ?? false);
                        cmd.Parameters.AddWithValue("@Tier2MonthlyPrice", GetDecimal(Tier2MonthlyPriceTextBox.Text));
                        cmd.Parameters.AddWithValue("@Tier2AnnualPrice", GetDecimal(Tier2AnnualPriceTextBox.Text));
                        cmd.Parameters.AddWithValue("@Tier2TermMonthly", Tier2TermMonthlyCheckBox.IsChecked ?? false);
                        cmd.Parameters.AddWithValue("@Tier2TermAnnual", Tier2TermAnnualCheckBox.IsChecked ?? false);

                        // Tier 3 Parameters
                        cmd.Parameters.AddWithValue("@Tier3Name", Tier3NameTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@Tier3Points", int.TryParse(Tier3PointsPerDollarTextBox.Text, out var tier3Points) ? tier3Points : 0);
                        cmd.Parameters.AddWithValue("@Tier3IsPaid", Tier3IsPaidCheckBox.IsChecked ?? false);
                        cmd.Parameters.AddWithValue("@Tier3MonthlyPrice", GetDecimal(Tier3MonthlyPriceTextBox.Text));
                        cmd.Parameters.AddWithValue("@Tier3AnnualPrice", GetDecimal(Tier3AnnualPriceTextBox.Text));
                        cmd.Parameters.AddWithValue("@Tier3TermMonthly", Tier3TermMonthlyCheckBox.IsChecked ?? false);
                        cmd.Parameters.AddWithValue("@Tier3TermAnnual", Tier3TermAnnualCheckBox.IsChecked ?? false);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Loyalty program saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving loyalty program: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal GetDecimal(string input)
        {
            return decimal.TryParse(input, out var value) ? value : 0;
        }


        private void Tier1IsPaidCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier1MonthlyPriceTextBox.Visibility = Tier1IsPaidCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            Tier1AnnualPriceTextBox.Visibility = Tier1IsPaidCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tier1TermMonthlyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier1MonthlyPriceTextBox.Visibility = Tier1TermMonthlyCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tier1TermAnnualCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier1AnnualPriceTextBox.Visibility = Tier1TermAnnualCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tier2IsPaidCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier2MonthlyPriceTextBox.Visibility = Tier2IsPaidCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            Tier2AnnualPriceTextBox.Visibility = Tier2IsPaidCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tier2TermMonthlyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier2MonthlyPriceTextBox.Visibility = Tier2TermMonthlyCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tier2TermAnnualCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier2AnnualPriceTextBox.Visibility = Tier2TermAnnualCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tier3IsPaidCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier3MonthlyPriceTextBox.Visibility = Tier3IsPaidCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            Tier3AnnualPriceTextBox.Visibility = Tier3IsPaidCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tier3TermMonthlyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier3MonthlyPriceTextBox.Visibility = Tier3TermMonthlyCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Tier3TermAnnualCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tier3AnnualPriceTextBox.Visibility = Tier3TermAnnualCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
