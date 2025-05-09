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
using System.Windows.Shapes;
using MerlinBackOffice.Helpers;
using MerlinBackOffice.Windows.InventoryWindows;

namespace MerlinBackOffice.Menus
{
    /// <summary>
    /// Interaction logic for CountsMenu.xaml
    /// </summary>
    public partial class CountsMenu : Window
    {
        public DatabaseHelper databaseHelper = new DatabaseHelper();
        public CountsMenu()
        {
            InitializeComponent();
        }

        private void OnBtnCategoryCount_Click(object sender, RoutedEventArgs e)
        {
            CategoryCountWindow categoryCountWindow = new CategoryCountWindow();
            categoryCountWindow.ShowDialog();
        }

        private void OnBtnDefectiveCount_Click(object sender, RoutedEventArgs e)
        {
            DefectiveCountWindow defectiveCountWindow = new DefectiveCountWindow();
            defectiveCountWindow.ShowDialog();
        }

        private void ResetCounts_Click(object sender, RoutedEventArgs e)
        {
            // Confirm reset action with the user
            var result = MessageBox.Show(
                "Are you sure you want to reset all counts? This action cannot be undone.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
                return;

            string connectionString = databaseHelper.GetConnectionString(); // Replace with your actual connection string

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL queries to delete all records from CountDetails and Counts
                    string deleteCountDetailsQuery = "DELETE FROM CountDetails;";
                    string deleteCountsQuery = "DELETE FROM Counts;";

                    // Execute both queries
                    using (SqlCommand command = new SqlCommand(deleteCountDetailsQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand(deleteCountsQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("All counts have been reset successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error resetting counts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
