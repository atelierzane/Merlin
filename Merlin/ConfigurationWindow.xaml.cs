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

namespace MerlinAdministrator
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        public ConfigurationWindow()
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.DatabaseConnection))
            {
                txtDatabaseConnection.Text = @"Server=.\SQLEXPRESS;Database=MerlinDatabase_Base;Trusted_Connection=True;";
            }
            else
            {
                txtDatabaseConnection.Text = Properties.Settings.Default.DatabaseConnection;
            }
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            string newConnectionString = txtDatabaseConnection.Text?.Trim();

            if (string.IsNullOrWhiteSpace(newConnectionString))
            {
                MessageBox.Show("Please enter a valid connection string.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate format using SqlConnectionStringBuilder
            try
            {
                var builder = new SqlConnectionStringBuilder(newConnectionString);

                // Optionally enforce required values
                if (string.IsNullOrWhiteSpace(builder.DataSource) ||
                    string.IsNullOrWhiteSpace(builder.InitialCatalog) ||
                    !builder.IntegratedSecurity)
                {
                    MessageBox.Show("Connection string must include Server, Database, and Trusted_Connection=True.", "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Save only if valid
                Properties.Settings.Default.DatabaseConnection = newConnectionString;
                Properties.Settings.Default.Save();
                MessageBox.Show("Database connection string updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid connection string format.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
