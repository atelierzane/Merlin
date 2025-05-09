using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinPointOfSale.Controls;

namespace MerlinPointOfSale.Pages.ReleasePerformancePages
{
    public partial class PerformanceKPIPage : Page
    {
        private DatabaseHelper databaseHelper = new DatabaseHelper();

        public PerformanceKPIPage()
        {
            InitializeComponent();
            this.Loaded += PerformanceDashboardPage_Loaded; // Ensure controls are initialized before use
        }

        private void PerformanceDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadKPIs(); // Only load KPIs after the page has fully loaded
        }

        private void LoadKPIs()
        {
            if (KPIStackPanel == null) return; // Safety check to avoid null reference

            string connectionString = databaseHelper.GetConnectionString();
            DateTime startDate, endDate;
            GetSelectedDateRange(out startDate, out endDate);

            KPIStackPanel.Children.Clear(); // Clear existing gauges

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT KPIID FROM KPI_Custom";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> kpiIDs = new List<string>();
                            while (reader.Read())
                            {
                                kpiIDs.Add(reader["KPIID"].ToString());
                            }

                            // Create and add a GaugeControl for each KPI
                            foreach (string kpiID in kpiIDs)
                            {
                                GaugeControl gauge = new GaugeControl();
                                gauge.LoadKPI(kpiID, connectionString, startDate, endDate); // Pass date range

                                // Add the control to the stack panel
                                KPIStackPanel.Children.Add(gauge);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading KPIs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadKPIs(); // Reload KPIs when time range changes
        }

        private void GetSelectedDateRange(out DateTime startDate, out DateTime endDate)
        {
            endDate = DateTime.Today;

            switch (TimeRangeComboBox.SelectedItem)
            {
                case ComboBoxItem item when item.Content.ToString() == "Today":
                    startDate = DateTime.Today;
                    break;

                case ComboBoxItem item when item.Content.ToString() == "Week to Date":
                    startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek); // Start of week
                    break;

                case ComboBoxItem item when item.Content.ToString() == "Month to Date":
                    startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // Start of month
                    break;

                default:
                    startDate = DateTime.Today;
                    break;
            }
        }
    }
}
