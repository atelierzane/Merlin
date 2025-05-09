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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MerlinAdministrator.Pages.LoyaltyManagerPages
{
    public partial class LoyaltySearchPage : Page
    {
        public DatabaseHelper dbHelper = new DatabaseHelper();

        public LoyaltySearchPage()
        {
            InitializeComponent();
        }

        // Load loyalty programs based on search criteria
        private void LoadLoyaltyPrograms(string programName, string tierName, (int? minPoints, int? maxPoints)? pointsRange)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        SELECT LoyaltyProgramName, LoyaltyTier1Name AS TierName, LoyaltyTier1PointsPerDollar AS PointsPerDollar, LoyaltyTier1MonthlyPrice AS MonthlyPrice, LoyaltyTier1AnnualPrice AS AnnualPrice
                        FROM Loyalty
                        WHERE 1=1";

                    // Apply filters
                    if (!string.IsNullOrWhiteSpace(programName))
                    {
                        query += " AND LoyaltyProgramName LIKE @ProgramName";
                    }
                    if (!string.IsNullOrWhiteSpace(tierName))
                    {
                        query += " AND (LoyaltyTier1Name LIKE @TierName OR LoyaltyTier2Name LIKE @TierName OR LoyaltyTier3Name LIKE @TierName)";
                    }
                    if (pointsRange.HasValue)
                    {
                        if (pointsRange.Value.minPoints.HasValue)
                        {
                            query += " AND (LoyaltyTier1PointsPerDollar >= @MinPoints OR LoyaltyTier2PointsPerDollar >= @MinPoints OR LoyaltyTier3PointsPerDollar >= @MinPoints)";
                        }
                        if (pointsRange.Value.maxPoints.HasValue)
                        {
                            query += " AND (LoyaltyTier1PointsPerDollar <= @MaxPoints OR LoyaltyTier2PointsPerDollar <= @MaxPoints OR LoyaltyTier3PointsPerDollar <= @MaxPoints)";
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters
                        if (!string.IsNullOrWhiteSpace(programName))
                            cmd.Parameters.AddWithValue("@ProgramName", $"%{programName}%");
                        if (!string.IsNullOrWhiteSpace(tierName))
                            cmd.Parameters.AddWithValue("@TierName", $"%{tierName}%");
                        if (pointsRange.HasValue)
                        {
                            if (pointsRange.Value.minPoints.HasValue)
                                cmd.Parameters.AddWithValue("@MinPoints", pointsRange.Value.minPoints);
                            if (pointsRange.Value.maxPoints.HasValue)
                                cmd.Parameters.AddWithValue("@MaxPoints", pointsRange.Value.maxPoints);
                        }

                        // Execute the query
                        List<LoyaltyProgram> loyaltyPrograms = new List<LoyaltyProgram>();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                loyaltyPrograms.Add(new LoyaltyProgram
                                {
                                    LoyaltyProgramName = reader["LoyaltyProgramName"].ToString(),
                                    TierName = reader["TierName"].ToString(),
                                    PointsPerDollar = Convert.ToInt32(reader["PointsPerDollar"]),
                                    MonthlyPrice = reader["MonthlyPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MonthlyPrice"]),
                                    AnnualPrice = reader["AnnualPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["AnnualPrice"])
                                });
                            }
                        }

                        // Bind the retrieved loyalty programs to the DataGrid
                        LoyaltyDataGrid.ItemsSource = loyaltyPrograms;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Search button click event
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string programName = ProgramNameTextBox.Text.Trim();
            string tierName = TierNameTextBox.Text.Trim();
            int? minPoints = null, maxPoints = null;

            // Try parsing the points values
            if (int.TryParse(MinPointsTextBox.Text, out int parsedMinPoints))
                minPoints = parsedMinPoints;
            if (int.TryParse(MaxPointsTextBox.Text, out int parsedMaxPoints))
                maxPoints = parsedMaxPoints;

            // Apply filters, if any
            LoadLoyaltyPrograms(programName, tierName, (minPoints, maxPoints));
        }

        // Reset button click event to clear filters
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramNameTextBox.Clear();
            TierNameTextBox.Clear();
            MinPointsTextBox.Clear();
            MaxPointsTextBox.Clear();

            LoyaltyDataGrid.ItemsSource = null; // Clear the grid
        }
    }

    // Loyalty Program Model
    public class LoyaltyProgram
    {
        public string LoyaltyProgramName { get; set; }
        public string TierName { get; set; }
        public int PointsPerDollar { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public decimal? AnnualPrice { get; set; }
    }
}
