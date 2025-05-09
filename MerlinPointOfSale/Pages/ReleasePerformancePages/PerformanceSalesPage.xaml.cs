using System;
using System.Collections.Generic;
using System.ComponentModel;          //  ◀─ add
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices; //  ◀─ add
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using MerlinPointOfSale.Helpers;

namespace MerlinPointOfSale.Pages.ReleasePerformancePages
{
    public partial class PerformanceSalesPage : Page, INotifyPropertyChanged  // ◀─ implement
    {
        private readonly MerlinPointOfSale.Helpers.DatabaseHelper db = new();

        // ─── Backing fields ─────────────────────────────────────────────────────
        private decimal _totalSales;
        private int _totalTransactions;
        private decimal _planSales = 1_000m;
        private SeriesCollection _salesSeries;
        private List<string> _dates;
        private SeriesCollection _salesByCategory;
        // ─────────────────────────────────────────────────────────────────────────

        public PerformanceSalesPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadPerformanceData();
        }

        // ─── Bindable Properties (with change notification) ─────────────────────
        public decimal TotalSales
        {
            get => _totalSales;
            set { _totalSales = value; OnPropertyChanged(); OnPropertyChanged(nameof(SalesVsPlanGaugeValue)); }
        }

        public int TotalTransactions
        {
            get => _totalTransactions;
            set { _totalTransactions = value; OnPropertyChanged(); }
        }

        public decimal PlanSales
        {
            get => _planSales;
            set { _planSales = value; OnPropertyChanged(); OnPropertyChanged(nameof(SalesVsPlanGaugeValue)); }
        }

        public double SalesVsPlanGaugeValue => PlanSales == 0 ? 0 : (double)(TotalSales / PlanSales);

        public Func<double, string> GaugeLabelFormatter => v => $"{v:P0}";
        public Func<double, string> Formatter => v => v.ToString("C");

        public SeriesCollection SalesSeries
        {
            get => _salesSeries;
            set { _salesSeries = value; OnPropertyChanged(); }
        }

        public List<string> Dates
        {
            get => _dates;
            set { _dates = value; OnPropertyChanged(); }
        }

        public SeriesCollection SalesByCategorySeries
        {
            get => _salesByCategory;
            set { _salesByCategory = value; OnPropertyChanged(); }
        }
        // ─────────────────────────────────────────────────────────────────────────

        // ─── Data loaders (unchanged except they now assign the properties) ─────
        private void LoadPerformanceData()
        {
            try
            {
                LoadTotals();
                LoadSalesOverTime();
                LoadSalesByCategory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load performance data:\n{ex.Message}",
                                "Performance", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTotals()
        {
            const string sql = @"
                SELECT ISNULL(SUM(TotalAmount),0) AS TotalSales,
                       COUNT(*)                   AS TotalTransactions
                FROM   Transactions
                WHERE  LocationID      = @Location
                AND    TransactionDate = @Day
                AND    IsSuspended     = 0
                AND    IsPostVoid      = 0";

            var table = db.ExecuteQuery(sql,
                          new SqlParameter("@Location", Properties.Settings.Default.LocationID),
                          new SqlParameter("@Day", DateTime.Today));

            if (table.Rows.Count == 1)
            {
                TotalSales = table.Rows[0].Field<decimal>("TotalSales");
                TotalTransactions = table.Rows[0].Field<int>("TotalTransactions");
            }
        }

        private void LoadSalesOverTime()
        {
            const string sql = @"
                SELECT FORMAT(TransactionTime,'HH') AS Hr,
                       SUM(TotalAmount)             AS Total
                FROM   Transactions
                WHERE  LocationID      = @Location
                AND    TransactionDate = @Day
                AND    IsSuspended     = 0
                AND    IsPostVoid      = 0
                GROUP  BY FORMAT(TransactionTime,'HH')
                ORDER  BY Hr";

            var table = db.ExecuteQuery(sql,
                          new SqlParameter("@Location", Properties.Settings.Default.LocationID),
                          new SqlParameter("@Day", DateTime.Today));

            var labels = new List<string>();
            var vals = new ChartValues<decimal>();

            foreach (DataRow r in table.Rows)
            {
                labels.Add($"{r["Hr"]}:00");
                vals.Add(r.Field<decimal>("Total"));
            }

            Dates = labels;
            SalesSeries = new SeriesCollection
            {
                new ColumnSeries { Title = "Sales", Values = vals }
            };
        }

        private void LoadSalesByCategory()
        {
            const string sql = @"
                SELECT cm.CategoryName,
                       SUM(td.Price * td.Quantity) AS CatSales
                FROM   TransactionDetails td
                       INNER JOIN Catalog     c  ON c.SKU       = td.SKU
                       INNER JOIN CategoryMap cm ON cm.CategoryID = c.CategoryID
                       INNER JOIN Transactions t  ON t.TransactionID = td.TransactionID
                WHERE  t.LocationID      = @Location
                AND    t.TransactionDate = @Day
                AND    t.IsSuspended     = 0
                AND    t.IsPostVoid      = 0
                GROUP  BY cm.CategoryName
                ORDER  BY CatSales DESC";

            var table = db.ExecuteQuery(sql,
                          new SqlParameter("@Location", Properties.Settings.Default.LocationID),
                          new SqlParameter("@Day", DateTime.Today));

            var pie = new SeriesCollection();

            foreach (DataRow r in table.Rows)
            {
                pie.Add(new PieSeries
                {
                    Title = r.Field<string>("CategoryName"),
                    Values = new ChartValues<decimal> { r.Field<decimal>("CatSales") },
                    DataLabels = true
                });
            }

            SalesByCategorySeries = pie;
        }
        // ─────────────────────────────────────────────────────────────────────────

        // ─── INotifyPropertyChanged plumbing ────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
