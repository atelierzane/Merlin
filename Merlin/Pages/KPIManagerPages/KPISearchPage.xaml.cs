using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.KPIManager
{
    public partial class KPISearchPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public KPISearchPage()
        {
            InitializeComponent();
            ResetFilters();
        }

        // Load KPIs based on search criteria
        private void LoadKPIs(string kpiName, string compareTo, string displayAs)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT KPIName, KPICompareTo, KPIDisplayAs, KPIPlan, KPIGoal FROM KPI_Custom WHERE 1 = 1";

                    // Apply filters
                    if (!string.IsNullOrWhiteSpace(kpiName))
                    {
                        query += " AND KPIName LIKE @KPIName";
                    }
                    if (!string.IsNullOrWhiteSpace(compareTo) && compareTo != "All")
                    {
                        query += " AND KPICompareTo = @KPICompareTo";
                    }
                    if (!string.IsNullOrWhiteSpace(displayAs) && displayAs != "All")
                    {
                        query += " AND KPIDisplayAs = @KPIDisplayAs";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters
                        if (!string.IsNullOrWhiteSpace(kpiName))
                            cmd.Parameters.AddWithValue("@KPIName", $"%{kpiName}%");
                        if (!string.IsNullOrWhiteSpace(compareTo) && compareTo != "All")
                            cmd.Parameters.AddWithValue("@KPICompareTo", compareTo);
                        if (!string.IsNullOrWhiteSpace(displayAs) && displayAs != "All")
                            cmd.Parameters.AddWithValue("@KPIDisplayAs", displayAs);

                        // Execute the query
                        List<KPI> kpis = new List<KPI>();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                kpis.Add(new KPI
                                {
                                    KPIName = reader["KPIName"].ToString(),
                                    KPICompareTo = reader["KPICompareTo"].ToString(),
                                    KPIDisplayAs = reader["KPIDisplayAs"].ToString(),
                                    KPIPlan = reader["KPIPlan"] != DBNull.Value ? Convert.ToDecimal(reader["KPIPlan"]) : (decimal?)null,
                                    KPIGoal = reader["KPIGoal"] != DBNull.Value ? Convert.ToDecimal(reader["KPIGoal"]) : (decimal?)null,
                                });
                            }
                        }

                        // Bind the retrieved KPIs to the DataGrid
                        KPIDataGrid.ItemsSource = kpis;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // KPI class to hold KPI data
        public class KPI
        {
            public string KPIName { get; set; }
            public string KPICompareTo { get; set; }
            public string KPIDisplayAs { get; set; }
            public decimal? KPIPlan { get; set; }
            public decimal? KPIGoal { get; set; }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string kpiName = KPINameTextBox.Text.Trim();
            string compareTo = (CompareToComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string displayAs = (DisplayAsComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            LoadKPIs(kpiName, compareTo, displayAs);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetFilters();
        }

        // Reset filters to default values
        private void ResetFilters()
        {
            KPINameTextBox.Clear();
            CompareToComboBox.SelectedIndex = 0; // Default to "All"
            DisplayAsComboBox.SelectedIndex = 0; // Default to "All"
            KPIDataGrid.ItemsSource = null; // Clear the grid
        }
    }
}
