using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MerlinPointOfSale.Properties;

namespace MerlinPointOfSale.Pages.ReleaseSchedulingPayrollPages
{
    public class TimeCardRecord
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Date { get; set; }
        public string ClockIn { get; set; }
        public string BreakStart { get; set; }
        public string BreakEnd { get; set; }
        public string ClockOut { get; set; }
    }

    public class EmployeeEarnings
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public decimal TotalTips { get; set; }
        public decimal TotalCommission { get; set; }
    }

    public partial class TimeCardsPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        // Get the default LocationID from settings (ensure this is set correctly)
        string LocationID = Settings.Default.LocationID;

        public TimeCardsPage()
        {
            InitializeComponent();
            PopulateWeekSelector();
            DisplayLocationInfo();
            // Compute current week-ending (Saturday) regardless of today's day.
            DateTime today = DateTime.Today;
            DateTime weekEnding = today.AddDays(6 - (int)today.DayOfWeek);
            LoadTimeCards(weekEnding);
        }

        private void PopulateWeekSelector()
        {
            DateTime currentDate = DateTime.Today;
            // Create a range: 5 weeks back to 5 weeks ahead.
            DateTime startDate = currentDate.AddDays(-(int)currentDate.DayOfWeek).AddDays(-35);
            DateTime endDate = currentDate.AddDays(-(int)currentDate.DayOfWeek).AddDays(35);

            while (startDate <= endDate)
            {
                DateTime weekStart = startDate;
                DateTime weekEnd = startDate.AddDays(6);
                WeekSelector.Items.Add(new ComboBoxItem
                {
                    Content = $"{weekStart:MM/dd/yyyy} - {weekEnd:MM/dd/yyyy}",
                    Tag = weekEnd
                });
                startDate = startDate.AddDays(7);
            }
            WeekSelector.SelectedIndex = WeekSelector.Items.Count / 2;
        }

        private void DisplayLocationInfo()
        {
            string locationID = Settings.Default.LocationID;
            LocationTextBlock.Text = $"Location ID: {locationID}";
        }

        private void WeekSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WeekSelector.SelectedItem is ComboBoxItem selectedWeek && selectedWeek.Tag is DateTime weekEnding)
            {
                LoadTimeCards(weekEnding);
            }
        }

        private void LoadTimeCards(DateTime weekEnding)
        {
            // For a week ending on Saturday, assume week starts on Sunday.
            DateTime weekStart = weekEnding.AddDays(-6);
            var timeCards = new List<TimeCardRecord>();
            var earnings = new List<EmployeeEarnings>();

            using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
            {
                conn.Open();

                // STEP 1: Load Time Cards (Shifts)
                string shiftQuery = @"
                    SELECT 
                        LTRIM(RTRIM(LT.EmployeeID)) AS EmployeeID,
                        LTRIM(RTRIM(E.EmployeeFirstName)) + ' ' + LTRIM(RTRIM(E.EmployeeLastName)) AS EmployeeName,
                        LT.TimePunchDate,
                        MAX(CASE WHEN LT.TimePunchType = 'Clock In' THEN LT.TimePunchTime END) AS ClockIn,
                        MAX(CASE WHEN LT.TimePunchType = 'Break Start' THEN LT.TimePunchTime END) AS BreakStart,
                        MAX(CASE WHEN LT.TimePunchType = 'Break End' THEN LT.TimePunchTime END) AS BreakEnd,
                        MAX(CASE WHEN LT.TimePunchType = 'Clock Out' THEN LT.TimePunchTime END) AS ClockOut
                    FROM LocationTimeCard LT
                    JOIN Employees E ON LTRIM(RTRIM(LT.EmployeeID)) = LTRIM(RTRIM(E.EmployeeID))
                    WHERE LT.LocationID = @LocationID
                      AND LT.TimePunchDate BETWEEN @StartDate AND @EndDate
                    GROUP BY LTRIM(RTRIM(LT.EmployeeID)), LTRIM(RTRIM(E.EmployeeFirstName)), LTRIM(RTRIM(E.EmployeeLastName)), LT.TimePunchDate
                    ORDER BY EmployeeName, LT.TimePunchDate;";

                using (SqlCommand cmd = new SqlCommand(shiftQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@LocationID", LocationID.Trim());
                    cmd.Parameters.AddWithValue("@StartDate", weekStart);
                    cmd.Parameters.AddWithValue("@EndDate", weekEnding);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            timeCards.Add(new TimeCardRecord
                            {
                                EmployeeID = reader["EmployeeID"].ToString(),
                                EmployeeName = reader["EmployeeName"].ToString(),
                                Date = Convert.ToDateTime(reader["TimePunchDate"]).ToString("MM/dd/yyyy"),
                                ClockIn = reader["ClockIn"] != DBNull.Value ? reader["ClockIn"].ToString() : "",
                                BreakStart = reader["BreakStart"] != DBNull.Value ? reader["BreakStart"].ToString() : "",
                                BreakEnd = reader["BreakEnd"] != DBNull.Value ? reader["BreakEnd"].ToString() : "",
                                ClockOut = reader["ClockOut"] != DBNull.Value ? reader["ClockOut"].ToString() : ""
                            });
                        }
                    }
                }

                // STEP 2: Load Earnings (Tips & Commissions)
                // Group solely by EmployeeID, using MAX() to retrieve the EmployeeName.
                string earningsQuery = @"
                                        SELECT
                                            LTRIM(RTRIM(T.EmployeeID)) AS EmployeeID,
                                            LTRIM(RTRIM(E.EmployeeFirstName)) + ' ' + LTRIM(RTRIM(E.EmployeeLastName)) AS EmployeeName,
                                            SUM(ISNULL(T.TipsTotal, 0)) AS TotalTips,
                                            SUM(ISNULL(T.CommissionTotal, 0)) AS TotalCommission
                                        FROM Transactions T
                                        JOIN Employees E ON LTRIM(RTRIM(T.EmployeeID)) = LTRIM(RTRIM(E.EmployeeID))
                                        WHERE RTRIM(T.LocationID) = @LocationID
                                          AND T.TransactionDate BETWEEN @StartDate AND @EndDate
                                        GROUP BY LTRIM(RTRIM(T.EmployeeID)),
                                                 LTRIM(RTRIM(E.EmployeeFirstName)) + ' ' + LTRIM(RTRIM(E.EmployeeLastName));";

                using (SqlCommand cmd = new SqlCommand(earningsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@LocationID", LocationID.Trim());
                    cmd.Parameters.AddWithValue("@StartDate", weekStart);
                    cmd.Parameters.AddWithValue("@EndDate", weekEnding);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            earnings.Add(new EmployeeEarnings
                            {
                                EmployeeID = reader["EmployeeID"].ToString(),
                                EmployeeName = reader["EmployeeName"].ToString(),
                                TotalTips = Convert.ToDecimal(reader["TotalTips"]),
                                TotalCommission = Convert.ToDecimal(reader["TotalCommission"])
                            });
                        }
                    }
                }
            }

            // STEP 3: Bind DataGrids
            TimeCardDataGrid.ItemsSource = timeCards;
            EarningsDataGrid.ItemsSource = earnings;

            // STEP 4: Display Total Summary
            decimal totalTips = earnings.Sum(e => e.TotalTips);
            decimal totalCommission = earnings.Sum(e => e.TotalCommission);
            LocationTotalHoursTextBlock.Text = $"Total Tips: {totalTips:C2} | Total Commission: {totalCommission:C2}";
        }
    }
}
