using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace MerlinPointOfSale.Controls
{
    public partial class GaugeControl : UserControl
    {
        public GaugeControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(GaugeControl), new PropertyMetadata("Gauge Title"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(double), typeof(GaugeControl), new PropertyMetadata(0.0));

        public double From
        {
            get => (double)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(double), typeof(GaugeControl), new PropertyMetadata(100.0));

        public double To
        {
            get => (double)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(GaugeControl), new PropertyMetadata(0.0));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty SummaryLabelProperty =
            DependencyProperty.Register("SummaryLabel", typeof(string), typeof(GaugeControl), new PropertyMetadata("Plan:"));

        public string SummaryLabel
        {
            get => (string)GetValue(SummaryLabelProperty);
            set => SetValue(SummaryLabelProperty, value);
        }

        public static readonly DependencyProperty SummaryValueProperty =
            DependencyProperty.Register("SummaryValue", typeof(double), typeof(GaugeControl), new PropertyMetadata(0.0));

        public double SummaryValue
        {
            get => (double)GetValue(SummaryValueProperty);
            set => SetValue(SummaryValueProperty, value);
        }

        public void LoadKPI(string kpiID, string connectionString, DateTime startDate, DateTime endDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT KPIName, KPITarget, KPIPlan, KPIGoal, KPICompareTo, KPIDisplayAs 
                             FROM KPI_Custom WHERE KPIID = @KPIID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@KPIID", kpiID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Title = reader["KPIName"].ToString();
                                string compareTo = reader["KPICompareTo"].ToString();
                                string displayAs = reader["KPIDisplayAs"].ToString();
                                double.TryParse(reader["KPIPlan"]?.ToString(), out double plan);
                                double.TryParse(reader["KPIGoal"]?.ToString(), out double goal);
                                string targetJson = reader["KPITarget"].ToString();

                                double actualValue = FetchActualValue(connectionString, compareTo, targetJson, startDate, endDate);

                                From = 0;
                                To = goal > 0 ? goal : (plan > 0 ? plan : 100);
                                Value = actualValue;
                                SummaryValue = plan;

                                UpdateDisplayFormat(displayAs);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading KPI data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double FetchActualValue(string connectionString, string compareTo, string targetJson, DateTime startDate, DateTime endDate)
        {
            string baseQuery = string.Empty;

            switch (compareTo)
            {
                case "Sales":
                    baseQuery = "SELECT SUM(TotalAmount) FROM Transactions WHERE TransactionDate BETWEEN @StartDate AND @EndDate AND IsSuspended = 0 AND IsPostVoid = 0";
                    break;

                case "Transactions":
                    baseQuery = "SELECT COUNT(*) FROM Transactions WHERE TransactionDate BETWEEN @StartDate AND @EndDate AND IsSuspended = 0 AND IsPostVoid = 0";
                    break;

                case "SKUs":
                    baseQuery = @"SELECT SUM(Quantity * Price) 
                          FROM TransactionDetails 
                          WHERE SKU IN (SELECT VALUE FROM OPENJSON(@TargetJson)) 
                          AND TransactionDate BETWEEN @StartDate AND @EndDate";
                    break;

                case "Categories":
                    baseQuery = @"SELECT SUM(Quantity * Price) 
                          FROM TransactionDetails 
                          WHERE CategoryID IN (SELECT VALUE FROM OPENJSON(@TargetJson)) 
                          AND TransactionDate BETWEEN @StartDate AND @EndDate";
                    break;
            }

            if (string.IsNullOrEmpty(baseQuery)) return 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(baseQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@TargetJson", targetJson);
                    object result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDouble(result) : 0;
                }
            }
        }



        private void UpdateDisplayFormat(string displayAs)
        {
            switch (displayAs)
            {
                case "Percentage":
                    Value = (Value / To) * 100;
                    SummaryValue = (SummaryValue / To) * 100;
                    break;
                case "Currency":
                    SummaryValue = Math.Round(SummaryValue, 2);
                    break;
                case "Number":
                default:
                    // No specific formatting
                    break;
            }
        }
    }
}
