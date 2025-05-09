using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace MerlinAdministrator.Pages.KPIManager
{
    public partial class AddKPIPage : Page
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();

        public AddKPIPage()
        {
            InitializeComponent();
        }

        // Handle target type change
        private void TargetTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (KPITargetTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string targetType = selectedItem.Content.ToString();
                LoadTargets(targetType, TargetItemsControl);
            }
        }

        // Handle dependency type change
        private void DependencyTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DependencyTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string dependencyType = selectedItem.Content.ToString();
                LoadTargets(dependencyType, DependencyItemsControl);
            }
        }

        // Load target or dependency items
        private void LoadTargets(string type, ItemsControl targetControl)
        {
            try
            {
                string query = type == "SKU"
                    ? "SELECT SKU AS ID, ProductName AS DisplayName FROM Catalog"
                    : "SELECT CategoryID AS ID, CategoryName AS DisplayName FROM CategoryMap";

                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    var items = new List<TargetItem>();
                    while (reader.Read())
                    {
                        items.Add(new TargetItem
                        {
                            ID = reader["ID"].ToString(),
                            DisplayName = reader["DisplayName"].ToString(),
                            IsSelected = false
                        });
                    }

                    targetControl.ItemsSource = items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Save KPI
        private void SaveKPI_Click(object sender, RoutedEventArgs e)
        {
            string kpiName = KPINameTextBox.Text.Trim();
            string kpiCompareTo = (KPICompareToComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string kpiDisplayAs = (KPIDisplayAsComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(kpiName) || string.IsNullOrEmpty(kpiCompareTo) || string.IsNullOrEmpty(kpiDisplayAs))
            {
                MessageBox.Show("KPI Name, Compare To, and Display As fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Parse optional plan and goal values
            decimal? kpiPlan = decimal.TryParse(KPIPlanTextBox.Text, out var planValue) ? planValue : (decimal?)null;
            string kpiPlanDisplayAs = kpiPlan.HasValue
                ? (KPIPlanDisplayAsComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                : null;

            decimal? kpiGoal = decimal.TryParse(KPIGoalTextBox.Text, out var goalValue) ? goalValue : (decimal?)null;
            string kpiGoalDisplayAs = kpiGoal.HasValue
                ? (KPIGoalDisplayAsComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                : null;

            // Collect selected targets
            var selectedTargets = ((IEnumerable<TargetItem>)TargetItemsControl.ItemsSource)?.Where(t => t.IsSelected).Select(t => t.ID).ToList();

            // Check for null or empty targets
            if (selectedTargets == null || !selectedTargets.Any())
            {
                MessageBox.Show("At least one target must be selected.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Collect selected dependencies (optional)
            var selectedDependencies = ((IEnumerable<TargetItem>)DependencyItemsControl.ItemsSource)?.Where(d => d.IsSelected).Select(d => d.ID).ToList();
            string kpiDependencyJson = (selectedDependencies != null && selectedDependencies.Any())
                ? JsonSerializer.Serialize(new { Type = DependencyTypeComboBox.SelectedItem?.ToString(), Values = selectedDependencies })
                : null;

            string targetType = (KPITargetTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var kpiTarget = new { Type = targetType, Values = selectedTargets };
            string kpiTargetJson = JsonSerializer.Serialize(kpiTarget);

            try
            {
                using (SqlConnection conn = new SqlConnection(dbHelper.GetConnectionString()))
                {
                    conn.Open();

                    string query = "INSERT INTO KPI_Custom (KPIID, KPIName, KPITarget, KPIDependency, KPIPlan, KPIPlanDisplayAs, KPIGoal, KPIGoalDisplayAs, KPICompareTo, KPIDisplayAs) " +
                                   "VALUES (NEWID(), @KPIName, @KPITarget, @KPIDependency, @KPIPlan, @KPIPlanDisplayAs, @KPIGoal, @KPIGoalDisplayAs, @KPICompareTo, @KPIDisplayAs)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@KPIName", kpiName);
                    cmd.Parameters.AddWithValue("@KPITarget", kpiTargetJson);
                    cmd.Parameters.AddWithValue("@KPIDependency", kpiDependencyJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@KPIPlan", kpiPlan ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@KPIPlanDisplayAs", kpiPlanDisplayAs ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@KPIGoal", kpiGoal ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@KPIGoalDisplayAs", kpiGoalDisplayAs ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@KPICompareTo", kpiCompareTo);
                    cmd.Parameters.AddWithValue("@KPIDisplayAs", kpiDisplayAs);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("KPI saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving KPI: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Clear form
        private void ClearForm()
        {
            KPINameTextBox.Clear();
            KPIPlanTextBox.Clear();
            KPIGoalTextBox.Clear();
            KPITargetTypeComboBox.SelectedIndex = -1;
            DependencyTypeComboBox.SelectedIndex = -1;
            TargetItemsControl.ItemsSource = null;
            DependencyItemsControl.ItemsSource = null;
        }
    }

    public class TargetItem
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }
        public bool IsSelected { get; set; }
    }
}
